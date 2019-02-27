using System;
using Autofac;
using DemiCode.Data.Scopes;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Autofac.Test
{
    [TestFixture]
    public class AssemblyModuleTest_WithRepoWithConcreteContext
    {
        private IContainer _container;

        public class SomeContext : IContext
        {
            public void Dispose()
            {
            }

            public void Commit()
            {
            }

            public void Seed()
            {
            }
        }

        public interface ISomeRepository
        {
            SomeContext ConcreteContext { get; }
        }

        public class SomeRepo : ISomeRepository
        {
            public SomeContext ConcreteContext { get; private set; }

            public SomeRepo(SomeContext concreteContext)
            {
                ConcreteContext = concreteContext;
            }
        }

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AssemblyModule>();
            builder.RegisterType<SomeRepo>().AsImplementedInterfaces();
            builder.RegisterType<SomeContext>().AsSelf().As<IContext>().ExternallyOwned();
            _container = builder.Build();
        }

        [Test]
        public void RepositoryFactory_RepoWithConcreteContextConstructorParameter()
        {
            var context = new SomeContext();
            var factory = _container.Resolve<Func<Type, IContext, object>>();

            var repository = (SomeRepo) factory(typeof (ISomeRepository), context);

            Assert.That(repository.ConcreteContext, Is.SameAs(context));
        }

        [Test]
        public void Scope_WithRepository_RepoIsInitializedWithConcreteContext()
        {
            var scope = _container.Resolve<IScope<ISomeRepository>>();
            
            scope.Commit(repo =>
            {
                Assert.That(repo.ConcreteContext, Is.InstanceOf<SomeContext>());
                return 0;
            });
        }

    }
}