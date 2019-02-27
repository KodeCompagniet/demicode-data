using System;
using System.Data.Entity.Infrastructure;
using Autofac;
using DemiCode.Data.Scopes;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Autofac.Test
{

    [TestFixture]
    public class AssemblyModuleTest_WithExecutionStrategy
    {
        private IContext _context;
        private IContainer _container;
        private IDbExecutionStrategy _executionStrategy;

        public class SomeRepo : AssemblyModuleTest_WithScopeFactory.ISomeRepository
        {
        }

        [SetUp]
        public void SetUp()
        {
            _executionStrategy = A.Fake<IDbExecutionStrategy>();
            _context = A.Fake<IContext>();

            var builder = new ContainerBuilder();
            builder.Register(c => _context);
            builder.RegisterModule(new AssemblyModule
            {
                ExecutionStrategyFactory = () => _executionStrategy
            });
            builder.RegisterType<SomeRepo>().AsImplementedInterfaces();
            _container = builder.Build();
        }

        [Test]
        public void ExecutionStrategy_WithReadOnlyScope_IsUsed()
        {
            var factory = _container.Resolve<ScopeFactory<AssemblyModuleTest_WithScopeFactory.ISomeRepository>>();
            var scope = factory();

            scope.ReadOnly(repo => 0);

            A.CallTo(() => _executionStrategy.Execute(A<Func<int>>._)).MustHaveHappened();
        }

        [Test]
        public void ExecutionStrategy_WithCommitScope_IsUsed()
        {
            var factory = _container.Resolve<ScopeFactory<AssemblyModuleTest_WithScopeFactory.ISomeRepository>>();
            var scope = factory();

            scope.Commit(repo => 0);

            A.CallTo(() => _executionStrategy.Execute(A<Func<int>>._)).MustHaveHappened();
        }

    }

}
