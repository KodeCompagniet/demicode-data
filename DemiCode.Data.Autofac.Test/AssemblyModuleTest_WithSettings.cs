using System.Transactions;
using Autofac;
using DemiCode.Data.Scopes;
using FakeItEasy;
using NUnit.Framework;
using IsolationLevel = System.Transactions.IsolationLevel;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Autofac.Test
{

    [TestFixture]
    public class AssemblyModuleTest_WithSettings
    {
        private IContext _context;
        private IContainer _container;

        public class SomeRepo : AssemblyModuleTest_WithScopeFactory.ISomeRepository
        {
        }

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            _context = A.Fake<IContext>();
            builder.Register(c => _context);
            builder.RegisterModule(new AssemblyModule
            {
                TransactionIsolationLevel = IsolationLevel.Snapshot
            });
            builder.RegisterType<SomeRepo>().AsImplementedInterfaces();
            _container = builder.Build();
        }

        [Test]
        public void TransactionIsolationLevel_WhenSet_IsUsedInReadOnlyScope()
        {
            var factory = _container.Resolve<ScopeFactory<AssemblyModuleTest_WithScopeFactory.ISomeRepository>>();
            var scope = factory();

            scope.ReadOnly(repo =>
            {
                Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.Snapshot));
                return 0;
            });
        }

        [Test]
        public void TransactionIsolationLevel_WhenSet_IsUsedInCommitScope()
        {
            var factory = _container.Resolve<ScopeFactory<AssemblyModuleTest_WithScopeFactory.ISomeRepository>>();
            var scope = factory();

            scope.Commit(repo =>
            {
                Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.Snapshot));
                return 0;
            });
        }


    }

}
