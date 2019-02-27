using System;
using System.Linq;
using DemiCode.Data.Test;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{

    [TestFixture]
    public class ScopeTest_Query
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
            A.CallTo(() => _repository.Query).Returns(new[] {0, 1, 2}.AsQueryable());

            _repositoryFactory = D.FakeFunc<Type, IContext, object>((type, scope) => _repository);

            _context = A.Fake<IContext>();
            _contextProvider = A.Fake<IContextProvider>();
            A.CallTo(() => _contextProvider.GetContext(A<Type>._)).Returns(_context);

            var scopeService = new ScopeService(_repositoryFactory.TheFunc, _contextProvider);

            _scope = scopeService.CreateScope<ScopeTest_WithOneRepository.IFakeRepository>();
        }

        [Test]
        public void Query_ContextIsNotDisposed()
        {
            var query = _scope.Query(repo => repo.Query);

            var data = query.ToList();

            A.CallTo(() => _context.Dispose())
                .MustNotHaveHappened();
        }

        [Test]
        public void Query_ContextIsNotKeptInScope()
        {
            _scope.Query(repo => repo.Query);

            Assert.That(_scope.CurrentContext, Is.Null);
        }


    }

}
