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
    public class AssemblyModuleTest_WithScopeFactory
    {
        private IContainer _container;
        private IContext _context;

        public interface ISomeRepository { }
        public interface ISomeOtherRepository { }
        public interface ITheThirdRepository { }

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            _context = A.Fake<IContext>();
            builder.Register(c => _context);
            builder.RegisterModule<AssemblyModule>();
            _container = builder.Build();
        }

        [Test]
        public void ScopeService_IsRegistered()
        {
            Assert.That(_container.IsRegistered<IScopeService>());
        }

        [Test]
        public void ContextFactory_IsRegistered()
        {
            Assert.That(_container.IsRegistered<Func<IContext>>());
        }

        [Test]
        public void UntypedRepositoryFactory_IsRegistered()
        {
            Assert.That(_container.IsRegistered<Func<Type, IContext, object>>());
        }
        
        [Test]
        public void ScopeFactory_WithOneRepository_IsRegistered()
        {
            Assert.That(_container.IsRegistered<ScopeFactory<ISomeRepository>>());
        }

        [Test]
        public void ScopeFactory_WithTwoRepositories_IsRegistered()
        {
            Assert.That(_container.IsRegistered<ScopeFactory<ISomeRepository, ISomeOtherRepository>>());
        }

        [Test]
        public void ScopeFactory_WithThreeRepositories_IsRegistered()
        {
            Assert.That(_container.IsRegistered<ScopeFactory<ISomeRepository, ISomeOtherRepository, ITheThirdRepository>>());
        }

        [Test]
        public void ScopeFactory_WithOneRepository_CanResolveScope()
        {
            var factory = _container.Resolve<ScopeFactory<ISomeRepository>>();
            
            var scope = factory();

            Assert.That(scope, Is.Not.Null & Is.InstanceOf<IScope<ISomeRepository>>());
        }

        [Test]
        public void ScopeFactory_WithAnotherRepository_CanResolveScope()
        {
            var factory = _container.Resolve<ScopeFactory<ISomeOtherRepository>>();

            var scope = factory();

            Assert.That(scope, Is.Not.Null & Is.InstanceOf<IScope<ISomeOtherRepository>>());
        }

        [Test]
        public void ScopeFactory_WithTwoRepositories_CanResolveScope()
        {
            var factory = _container.Resolve<ScopeFactory<ISomeRepository, ISomeOtherRepository>>();
            
            var scope = factory();

            Assert.That(scope, Is.Not.Null & Is.InstanceOf<IScope<ISomeRepository, ISomeOtherRepository>>());
        }

        [Test]
        public void ScopeFactory_WithThreeRepositories_CanResolveScope()
        {
            var factory = _container.Resolve<ScopeFactory<ISomeRepository, ISomeOtherRepository, ITheThirdRepository>>();

            var scope = factory();
            
            Assert.That(scope,
                        Is.Not.Null & Is.InstanceOf<IScope<ISomeRepository, ISomeOtherRepository, ITheThirdRepository>>());
        }

        [Test]
        public void Scope_WithOneRepository_CanBeResolvedDirectly()
        {
            var scope = _container.Resolve<IScope<ISomeRepository>>();

            Assert.That(scope, Is.Not.Null);
        }

        [Test]
        public void Scope_WithTwoRepositories_CanBeResolvedDirectly()
        {
            var scope = _container.Resolve<IScope<ISomeRepository, ISomeOtherRepository>>();

            Assert.That(scope, Is.Not.Null);
        }

        [Test]
        public void Scope_WithThreeRepositories_CanBeResolvedDirectly()
        {
            var scope = _container.Resolve<IScope<ISomeRepository, ISomeOtherRepository, ITheThirdRepository>>();

            Assert.That(scope, Is.Not.Null);
        }
    }
}