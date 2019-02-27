using System;
using Autofac;
using DemiCode.Data.Scopes;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Autofac.Test
{
    [TestFixture]
    public class AssemblyModuleTest_WithRepoWithAdditionalDependencies
    {
        private IContainer _container;
        private IContext _context;
        private ISomeDependency _someDependency;

        public interface ISomeRepository
        {
        }

        public interface ISomeDependency
        {
        }

        public class SomeRepo : ISomeRepository
        {
            public SomeRepo(IContext concreteContext, ISomeDependency someDependency)
            {
                OtherDependency = someDependency;
            }

            public ISomeDependency OtherDependency { get; private set; }
        }

        [SetUp]
        public void SetUp()
        {
            _context = A.Fake<IContext>();
            _someDependency = A.Fake<ISomeDependency>();

            var builder = new ContainerBuilder();
            builder.RegisterModule<AssemblyModule>();
            builder.RegisterType<SomeRepo>().AsImplementedInterfaces();
            builder.RegisterInstance(_context);
            builder.RegisterInstance(_someDependency);
            _container = builder.Build();
        }

        [Test]
        public void RepositoryFactory_RepoWithOtherDependency_CanBeResolved()
        {
            var factory = _container.Resolve<Func<Type, IContext, object>>();

            var repository = (SomeRepo)factory(typeof(ISomeRepository), _context);

            Assert.That(repository.OtherDependency, Is.SameAs(_someDependency));
        }


    }
}