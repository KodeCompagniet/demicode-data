using System;
using Autofac;
using DemiCode.Data.Scopes;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Autofac.Test
{
    [TestFixture]
    public class AssemblyModuleTest_WithDifferentContextsAndRepos
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AssemblyModule>();
            builder.RegisterType<Repo1>().AsImplementedInterfaces();
            builder.RegisterType<Repo2>().AsImplementedInterfaces();
            builder.RegisterType<Repo3>().AsImplementedInterfaces();
            builder.RegisterType<Context1>().AsSelf();
            builder.RegisterType<Context2>().AsSelf();
            builder.Register<Func<Type, IContext>>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return repoType => repoType == typeof(IFirstRepository) || repoType == typeof(IThirdRepository) ? c.Resolve<Context1>() : null;
            });
            builder.Register<Func<Type, IContext>>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return repoType => repoType == typeof(ISecondRepository) ? c.Resolve<Context2>() : null;
            });
            _container = builder.Build();
        }

        [Test]
        public void ResolveScope_WithDifferingContextsAndRepos()
        {
            var scope1 = _container.Resolve<IScope<IFirstRepository>>();
            var scope2 = _container.Resolve<IScope<ISecondRepository>>();

            scope1.ReadOnly(r1 => { Assert.That(r1.Context, Is.InstanceOf<Context1>()); return 0; });
            scope2.ReadOnly(r2 => { Assert.That(r2.Context, Is.InstanceOf<Context2>()); return 0; });
        }

        [Test]
        public void ResolveScope_WhenNestedInOtherScope_Throws()
        {
            var scope1 = _container.Resolve<IScope<IFirstRepository>>();
            var scope2 = _container.Resolve<IScope<ISecondRepository>>();

            scope1.ReadOnly(r1 =>
            {
                var ex = Assert.Throws<IncompatibleRepositoriesException>(() => scope2.ReadOnly(r2 => 42));

                Assert.That(ex.RepositoryType, Is.EqualTo(typeof(ISecondRepository)));
                Assert.That(ex.ContextType, Is.EqualTo(typeof(Context1)));

                return 0;
            });
        }

        [Test]
        public void ResolveScope_WithNestedCompatibleRepositories_DoesNotThrow()
        {
            var scope1 = _container.Resolve<IScope<IFirstRepository>>();
            var scope2 = _container.Resolve<IScope<IThirdRepository>>();

            scope1.ReadOnly(r1 =>
            {
                Assert.DoesNotThrow(() => scope2.ReadOnly(r2 => 42));
                return 0;
            });
        }

        [Test]
        public void ResolveScope_WithNestedCompatibleRepositories_SharesContextInstance()
        {
            var scope1 = _container.Resolve<IScope<IFirstRepository>>();
            var scope2 = _container.Resolve<IScope<IThirdRepository>>();

            scope1.ReadOnly(r1 =>
            {
                var outerContext = r1.Context;
                scope2.ReadOnly(r2 =>
                {
                    Assert.That(r2.Context, Is.SameAs(outerContext));
                    return 0;
                });
                return 0;
            });
        }

        [Test]
        public void ResolveScope_WithDualCompatibleRepositories_SharesContextInstance()
        {
            var scope = _container.Resolve<IScope<IFirstRepository, IThirdRepository>>();

            scope.ReadOnly((r1, r3) =>
            {
                Assert.That(r1.Context, Is.SameAs(r3.Context));
                return 0;
            });
        }

        [Test]
        public void ResolveScope_WithDualIncompatibleRepositories_Throws()
        {
            var scope = _container.Resolve<IScope<IFirstRepository, ISecondRepository>>();

            var ex = Assert.Throws<IncompatibleRepositoriesException>(() => scope.ReadOnly((r1, r2) => 0));
            Assert.That(ex.RepositoryType, Is.EqualTo(typeof(ISecondRepository)));
            Assert.That(ex.ContextType, Is.EqualTo(typeof(Context1)));
        }


        public class Context1 : IContext
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

        public class Context2 : IContext
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

        public interface IFirstRepository
        {
            Context1 Context { get; }
        }

        public interface ISecondRepository
        {
            Context2 Context { get; }
        }
        public interface IThirdRepository
        {
            Context1 Context { get; }
        }

        public class Repo1 : IFirstRepository
        {
            public Context1 Context { get; private set; }

            public Repo1(Context1 context)
            {
                Context = context;
            }
        }

        public class Repo2 : ISecondRepository
        {
            public Context2 Context { get; private set; }

            public Repo2(Context2 context)
            {
                Context = context;
            }
        }

        public class Repo3 : IThirdRepository
        {
            public Context1 Context { get; private set; }

            public Repo3(Context1 context)
            {
                Context = context;
            }
        }
    }
}