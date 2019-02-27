using System;
using System.Data.Entity.Infrastructure;
using DemiCode.Data.Test;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{
    [TestFixture]
    public class ScopeTest_WithTwoRepositories
    {
        private D.Func<Type, IContext, object> _repositoryFactory;
        private IContext _context;
        private IScope<IFakeRepository1, IFakeRepository2> _scope;
        private IFakeRepository1 _fakeRepo1;
        private IFakeRepository2 _fakeRepo2;
        private IContextProvider _contextProvider;

        [SetUp]
        public void SetUp()
        {
            _fakeRepo1 = A.Fake<IFakeRepository1>();
            _fakeRepo2 = A.Fake<IFakeRepository2>();

            _repositoryFactory = D.FakeFunc<Type, IContext, object>((type, scope) =>
            {
                if (type == typeof(IFakeRepository1))
                    return _fakeRepo1;
                if (type == typeof(IFakeRepository2))
                    return _fakeRepo2;
                throw new NotSupportedException(type.FullName + " is not a valid repository interface");
            });

            _context = A.Fake<IContext>();
            _contextProvider = A.Fake<IContextProvider>();
            A.CallTo(() => _contextProvider.GetContext(A<Type>._)).Returns(_context);

            var scopeService = new ScopeService(_repositoryFactory.TheFunc, _contextProvider);

            _scope = scopeService.CreateScope<IFakeRepository1, IFakeRepository2>();
        }

        [Test]
        public void ReadOnly_ExecutesWork()
        {
            var result = _scope.ReadOnly((repo1, repo2) => "the result");

            Assert.That(result, Is.EqualTo("the result"));
        }

        [Test]
        public void Commit_ExecutesWork()
        {
            var result = _scope.Commit((repo1, repo2) => "the result");

            Assert.That(result, Is.EqualTo("the result"));
        }

        [Test]
        public void ReadOnly_DoesNotCallCommitOnContext()
        {
            _scope.ReadOnly((repo1, repo2) => "");

            A.CallTo(() => _context.Commit()).MustNotHaveHappened();
        }

        [Test]
        public void Commit_CallsCommitOnContext()
        {
            _scope.Commit((repo1, repo2) => "");

            A.CallTo(() => _context.Commit()).MustHaveHappened();
        }

        [Test]
        public void ReadOnly_UsesRepositoryFactory()
        {
            _scope.ReadOnly((repo1, repo2) => "");

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(2));
            Assert.That(_repositoryFactory.In1[0], Is.EqualTo(typeof(IFakeRepository1)));
            Assert.That(_repositoryFactory.In1[1], Is.EqualTo(typeof(IFakeRepository2)));
            Assert.That(_repositoryFactory.In2[0], Is.Not.Null & Is.InstanceOf<IContext>());
            Assert.That(_repositoryFactory.In2[1], Is.Not.Null & Is.InstanceOf<IContext>());
        }

        [Test]
        public void Commit_UsesRepositoryFactory()
        {
            _scope.Commit((repo1, repo2) => "");

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(2));
            Assert.That(_repositoryFactory.In1[0], Is.EqualTo(typeof(IFakeRepository1)));
            Assert.That(_repositoryFactory.In1[1], Is.EqualTo(typeof(IFakeRepository2)));
            Assert.That(_repositoryFactory.In2[0], Is.Not.Null & Is.InstanceOf<IContext>());
            Assert.That(_repositoryFactory.In2[1], Is.Not.Null & Is.InstanceOf<IContext>());
        }

        [Test]
        public void Commit_WithOneRepository_PassesRepositoryToWorker()
        {
            _scope.Commit((repo1, repo2) =>
            {
                Assert.That(repo1, Is.Not.Null & Is.InstanceOf<IFakeRepository1>());
                Assert.That(repo2, Is.Not.Null & Is.InstanceOf<IFakeRepository2>());
                return "";
            });
            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void ReadOnly_PassesRepositoryToWorker()
        {
            _scope.ReadOnly((repo1, repo2) =>
            {
                Assert.That(repo1, Is.Not.Null & Is.InstanceOf<IFakeRepository1>());
                Assert.That(repo2, Is.Not.Null & Is.InstanceOf<IFakeRepository2>());
                return "";
            });

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(2));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Commit_WithConcurrencyHandler_CallsHandlerOnConcurrencyException(int concurrencyHandledResult)
        {
            A.CallTo(() => _context.Commit()).Throws(new DbUpdateConcurrencyException());

            var result = _scope.Commit((r1, r2) => 0, onConcurrencyError: (r1, r2) => concurrencyHandledResult);

            Assert.That(result, Is.EqualTo(concurrencyHandledResult));
        }

        [Test]
        public void Commit_WithoutConcurrencyHandler_ConcurrencyExceptionsAreThrown()
        {
            A.CallTo(() => _context.Commit()).Throws(new DbUpdateConcurrencyException());

            Assert.Throws<ConcurrencyException>(() => _scope.Commit((r1, r2) => 0));
        }

        [Test]
        public void CreatingCommitScope_WithThreeRepo_CommitWorkIsDoneOnAllThreeRepos()
        {
            _scope.Commit((repo1, repo2) =>
            {
                repo1.CommitMethod1();
                repo2.CommitMethod2();
                return 0;
            });

            A.CallTo(() => _fakeRepo1.CommitMethod1()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _fakeRepo2.CommitMethod2()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestCase(1, 1, 2)]
        [TestCase(2, 1, 3)]
        [TestCase(1, 2, 3)]
        [TestCase(5, 4, 9)]
        public void CreatingCommitAndNonCommitScope_WithThreeRepo_DoesBothCommitWorkAndNonCommitWorkOnAllRepos(int repo1Res, int repo2Res, int accumulatedRes)
        {
            A.CallTo((() => _fakeRepo1.CommitMethod1()))
                .Invokes(x =>
                    //If the commit method is called first, the non-commiting method will be allowed to returned the "updated" data
                    A.CallTo(() => _fakeRepo1.GetTheCommitedMethod1()).Returns(repo1Res))
                .Returns(0);
            A.CallTo((() => _fakeRepo2.CommitMethod2()))
                .Invokes(x =>
                    A.CallTo(() => _fakeRepo2.GetTheCommitedMethod2()).Returns(repo2Res))
                .Returns(0);


            var result = _scope.Commit(
                (repo1, repo2) =>
                {
                    repo1.CommitMethod1();
                    repo2.CommitMethod2();
                    return 0;
                },
                (repo1, repo2) =>
                {
                    var res1 = repo1.GetTheCommitedMethod1();
                    var res2 = repo2.GetTheCommitedMethod2();
                    return res1 + res2;
                });

            Assert.That(result, Is.EqualTo(accumulatedRes));
        }

        public interface IFakeRepository1
        {
            int CommitMethod1();
            int GetTheCommitedMethod1();
        }
        public interface IFakeRepository2
        {
            int CommitMethod2();
            int GetTheCommitedMethod2();
        }
    }
}