using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using DemiCode.Data.Repositories.Test;
using DemiCode.Data.Test;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{
    [TestFixture]
    public class ScopeTest_WithOneRepository
    {
        private D.Func<Type, IContext, object> _repositoryFactory;
        private IContext _context;
        private IScope<IFakeRepository> _scope;
        private IFakeRepository _repository;
        private IContextProvider _contextProvider;

        [SetUp]
        public void SetUp()
        {
            _repository = A.Fake<IFakeRepository>();
            _repositoryFactory = D.FakeFunc<Type, IContext, object>((type, scope) => _repository);

            _context = A.Fake<IFakeContext>();
            _contextProvider = A.Fake<IContextProvider>();
            A.CallTo(() => _contextProvider.GetContext(A<Type>._)).Returns(_context);

            var scopeService = new ScopeService(_repositoryFactory.TheFunc, _contextProvider);

            _scope = scopeService.CreateScope<IFakeRepository>();
        }

        [Test]
        public void ReadOnly_ExecutesWork()
        {
            var result = _scope.ReadOnly(repo => "the result");

            Assert.That(result, Is.EqualTo("the result"));
        }

        [Test]
        public void Commit_ExecutesWork()
        {
            var result = _scope.Commit(repo => "the result");

            Assert.That(result, Is.EqualTo("the result"));
        }

        [Test]
        public void ReadOnly_DoesNotCallCommitOnContext()
        {
            _scope.ReadOnly(repo => "");

            A.CallTo(() => _context.Commit()).MustNotHaveHappened();
        }

        [Test]
        public void Commit_CallsCommitOnContext()
        {
            _scope.Commit(repo => "");

            A.CallTo(() => _context.Commit()).MustHaveHappened();
        }

        [Test]
        public void ReadOnly_UsesRepositoryFactory()
        {
            _scope.ReadOnly(repo => "");

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(1));
            Assert.That(_repositoryFactory.In1[0], Is.EqualTo(typeof(IFakeRepository)));
            Assert.That(_repositoryFactory.In2[0], Is.Not.Null & Is.InstanceOf<IContext>());
        }

        [Test]
        public void Commit_UsesRepositoryFactory()
        {
            _scope.Commit(repo => "");

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(1));
            Assert.That(_repositoryFactory.In1[0], Is.EqualTo(typeof(IFakeRepository)));
            Assert.That(_repositoryFactory.In2[0], Is.Not.Null & Is.InstanceOf<IContext>());
        }

        [Test]
        public void Commit_PassesRepositoryToWorker()
        {
            _scope.Commit(repo =>
            {
                Assert.That(repo, Is.Not.Null & Is.InstanceOf<IFakeRepository>());
                return "";
            });

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void ReadOnly_PassesRepositoryToWorker()
        {
            _scope.ReadOnly(repo =>
            {
                Assert.That(repo, Is.Not.Null & Is.InstanceOf<IFakeRepository>());
                return "";
            });

            Assert.That(_repositoryFactory.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Commit_WithConcurrencyHandler_CallsHandlerOnConcurrencyException()
        {
            A.CallTo(() => _context.Commit()).Throws(new DbUpdateConcurrencyException());

            var result = _scope.Commit(repo => 0, onConcurrencyError: repo => 1);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void Commit_WithoutConcurrencyHandler_ConcurrencyExceptionsAreThrown()
        {
            A.CallTo(() => _context.Commit()).Throws(new DbUpdateConcurrencyException());

            Assert.Throws<ConcurrencyException>(() => _scope.Commit(repo => 0));
        }

        [Test]
        public void CreatingCommitScope_WithOneRepo_CommitWorkIsDone()
        {
            _scope.Commit(repo => repo.CommitMethod());

            A.CallTo(() => _repository.CommitMethod()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void CreatingCommitAndNonCommitScope_WithOneRepo_DoesBothCommitWorkAndNonCommitWork(int anyCommitUpdate)
        {
            A.CallTo((() => _repository.CommitMethod()))
                .Invokes(x =>
                    //If the commit method is called first, the non-commiting method will be allowed to returned the "updated" data
                    A.CallTo(() => _repository.GetTheCommitedMethod()).Returns(anyCommitUpdate))
                .Returns(0);

            var result = _scope.Commit(repo => repo.CommitMethod(), repo => repo.GetTheCommitedMethod());

            Assert.That(result, Is.EqualTo(anyCommitUpdate));
        }

        public interface IFakeRepository
        {
            int CommitMethod();
            int GetTheCommitedMethod();

            IQueryable<int> Query { get; }
        }
    }
}