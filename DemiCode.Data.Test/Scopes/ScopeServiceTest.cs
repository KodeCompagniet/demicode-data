using System;
using DemiCode.Data.Test;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{

    [TestFixture]
    public class ScopeServiceTest
    {
        private ScopeService _scopeService;
        private D.Func<Type, IContext, object> _repositoryFactory;
        private IContext _context;
        private IContextProvider _contextProvider;

        public interface ISomeRepository
        { }

        [SetUp]
        public void SetUp()
        {
            _repositoryFactory = D.FakeFunc<Type, IContext, object>((type, scope) => null);
            _context = A.Fake<IContext>();
            
            _contextProvider = A.Fake<IContextProvider>();
            A.CallTo(() => _contextProvider.GetContext(A<Type>._)).Returns(_context);

            _scopeService = new ScopeService(_repositoryFactory.TheFunc, _contextProvider);
        }

        [Test]
        public void CreateScope_WithOneRepository_CanBeCreated()
        {
            var scope = _scopeService.CreateScope<ISomeRepository>();
            Assert.That(scope, Is.Not.Null);
        }

    }
}
