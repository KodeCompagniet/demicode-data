using System;
using DemiCode.Data.Repositories.Test;
using DemiCode.Data.Test;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{
    [TestFixture]
    public class ScopeTest_WithConcreteRepository
    {
        private D.Func<Type, IContext, object> _repositoryFactory;
        private IContext _context;
        private IScope<IFakeRepository> _scope;
        private IContextProvider _contextProvider;

        [SetUp]
        public void SetUp()
        {
            _repositoryFactory = D.FakeFunc<Type, IContext, object>((type, context) => new FakeRepository((IFakeContext) context));

            _context = new FakeDataContext();
            _contextProvider = A.Fake<IContextProvider>();
            A.CallTo(() => _contextProvider.GetContext(A<Type>._)).Returns(_context);

            var scopeService = new ScopeService(_repositoryFactory.TheFunc, _contextProvider);

            _scope = scopeService.CreateScope<IFakeRepository>();
        }

        [Test]
        public void Scope_WithSpecializedContext_ContextIsPassedToRepository()
        {
            _scope.Commit(repo =>
            {
                Assert.That(repo.Context, Is.SameAs(_context));
                return 0;
            });
        }

    }
}