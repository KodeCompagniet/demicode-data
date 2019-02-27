using System;
using DemiCode.Data.Test;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{

    [TestFixture]
    public class ScopeTest
    {

        private D.Func<Type, IContext, object> _repositoryFactory;
        private IContext _context;
        private IScope<ScopeTest_WithOneRepository.IFakeRepository> _scope;
        private ScopeTest_WithOneRepository.IFakeRepository _repository;
        private IContextProvider _contextProvider;

        [SetUp]
        public void SetUp()
        {
            _repository = A.Fake<ScopeTest_WithOneRepository.IFakeRepository>();
            _repositoryFactory = D.FakeFunc<Type, IContext, object>((type, scope) => _repository);

            _context = A.Fake<IContext>();
            _contextProvider = A.Fake<IContextProvider>();
            A.CallTo(() => _contextProvider.GetContext(A<Type>._)).Returns(_context);

            var scopeService = new ScopeService(_repositoryFactory.TheFunc, _contextProvider);

            _scope = scopeService.CreateScope<ScopeTest_WithOneRepository.IFakeRepository>();
        }

        [Test]
        public void CurrentContext_WithinCommitScope_ReturnsCurrentContext()
        {
            _scope.Commit(repo =>
            {
                Assert.That(_scope.CurrentContext, Is.SameAs(_context));
                return 0;
            });
        }

        [Test]
        public void CurrentContext_WithinReadOnlyScope_ReturnsCurrentContext()
        {
            _scope.ReadOnly(repo =>
            {
                Assert.That(_scope.CurrentContext, Is.SameAs(_context));
                return 0;
            });
        }

        [Test]
        public void CurrentContext_OutsideScope_IsNull()
        {
            Assert.That(_scope.CurrentContext, Is.Null);
        }
         

    }

}
