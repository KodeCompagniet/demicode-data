using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using DemiCode.Data.Scopes;
using Module = Autofac.Module;
using System.Transactions;

namespace DemiCode.Data.Autofac
{
    /// <summary>
    /// Module for configuring Scope services in an Autofac container.
    /// </summary>
    public class AssemblyModule : Module
    {
        /// <summary>
        /// Isolation level to use in scope transactions.
        /// </summary>
        /// <value>Default value is <see cref="IsolationLevel.ReadCommitted"/></value>
        public IsolationLevel TransactionIsolationLevel { get; set; }

        /// <summary>
        /// Execution strategy factory to use when executing against the database. If set, an execution strategy will be made and the outermost transaction scope will be wrapped in <see cref="IDbExecutionStrategy.Execute"/>.
        /// </summary>
        /// <remarks>If null (default), or the factory returns null, no strategy will be used</remarks>
        public Func<IDbExecutionStrategy> ExecutionStrategyFactory { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(CreateRepositoryFactory);

            ScopeService.TransactionIsolationLevel = TransactionIsolationLevel;
            ScopeService.ExecutionStrategyFactory = ExecutionStrategyFactory;

            builder.RegisterType<MultipleRegistrationsContextProvider>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<ScopeService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterSource(new ScopeFactoryDelegateSource());
            builder.RegisterSource(new ScopeSource());
        }

        private static Func<Type, IContext, object> CreateRepositoryFactory(IComponentContext temporaryContext)
        {
            var componentContext = temporaryContext.Resolve<IComponentContext>();
            return (type, context) => componentContext.Resolve(type, new PositionalParameter(0, context));
        }



        /// <summary>
        /// Base class for registration sources that generate registrations for generic types.
        /// Override and implement <see cref="GetSupportedTypes"/> to return the open generic types the source supports.
        /// Also, for each type, implement a generic method that will be called to create the component when resolved for.
        /// 
        /// The creation method must follow this signature:
        /// private static GenericType&gt;TParam&lt; CreateComponent&gt;TParam&lt;(IComponentContext context) 
        /// </summary>
        internal abstract class GenericSourceBase : IRegistrationSource
        {
            protected abstract Type[] GetSupportedTypes();

            public bool IsAdapterForIndividualComponents { get { return true; } }

            public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                var swt = service as IServiceWithType;
                if (swt == null || !swt.ServiceType.IsGenericType)
                    yield break;

                var definition = swt.ServiceType.GetGenericTypeDefinition();
                if (!GetSupportedTypes().Any(type => type == definition))
                    yield break;

                var concreteDelegateType = swt.ServiceType;

                yield return RegistrationBuilder.ForDelegate(swt.ServiceType, (c, p) => CreateScopeFactoryFromGeneric(c, concreteDelegateType))
                    .As(service)
                    .CreateRegistration();
            }


            private object CreateScopeFactoryFromGeneric(IComponentContext temporaryComponentContext, Type delegateType)
            {
                var genericArguments = delegateType.GetGenericArguments().ToArray();
                var openGenericMethod = GetType()
                    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(mi => mi.IsGenericMethod)
                    .Where(mi => mi.Name == "CreateComponent").FirstOrDefault(mi => mi.GetGenericArguments().Count() == genericArguments.Length);

                if (openGenericMethod == null)
                    throw new InvalidOperationException("No CreateComponent method existed for the requested delegate type " + delegateType);

                var genericMethod = openGenericMethod.MakeGenericMethod(genericArguments);
                var context = temporaryComponentContext.Resolve<IComponentContext>();
                return genericMethod.Invoke(this, new object[] { context });
            }

        }

        /// <summary>
        /// Registration source that creates scope factory delegates when the closed generic delegates are requested from the container.
        /// </summary>
        internal class ScopeFactoryDelegateSource : GenericSourceBase
        {
            private readonly Type[] _supportedScopeFactoryTypes = {
                                                                      typeof (ScopeFactory<>),
                                                                      typeof (ScopeFactory<,>),
                                                                      typeof (ScopeFactory<,,>)
                                                                  };

            protected override Type[] GetSupportedTypes()
            {
                return _supportedScopeFactoryTypes;
            }

// ReSharper disable UnusedMember.Local
            /// <remarks>
            /// This method is indirectly accessed by <see cref="GenericSourceBase"/>.
            /// </remarks>>
            private static ScopeFactory<TRepository> CreateComponent<TRepository>(IComponentContext context) where TRepository : class
// ReSharper restore UnusedMember.Local
            {
                return () =>
                           {
                               var ss = context.Resolve<IScopeService>();
                               return ss.CreateScope<TRepository>();
                           };
            }

// ReSharper disable UnusedMember.Local
            /// <remarks>
            /// This method is indirectly accessed by <see cref="GenericSourceBase"/>.
            /// </remarks>>
            private static ScopeFactory<TRepository1, TRepository2> CreateComponent<TRepository1, TRepository2>(IComponentContext context) 
                where TRepository1 : class
                where TRepository2 : class
// ReSharper restore UnusedMember.Local
            {
                return () =>
                           {
                               var ss = context.Resolve<IScopeService>();
                               return ss.CreateScope<TRepository1, TRepository2>();
                           };
            }
            
// ReSharper disable UnusedMember.Local
            /// <remarks>
            /// This method is indirectly accessed by <see cref="GenericSourceBase"/>.
            /// </remarks>>
            private static ScopeFactory<TRepository1, TRepository2, TRepository3> CreateComponent<TRepository1, TRepository2, TRepository3>(IComponentContext context) 
                where TRepository1 : class
                where TRepository2 : class
                where TRepository3 : class
// ReSharper restore UnusedMember.Local
            {
                return () =>
                           {
                               var ss = context.Resolve<IScopeService>();
                               return ss.CreateScope<TRepository1, TRepository2, TRepository3>();
                           };
            }

        }

        /// <summary>
        /// Registration source that creates scope instances when the closed generic counterparts are requested from the container.
        /// The instances are created by first resolving a ScopeFactory
        /// </summary>
        internal class ScopeSource : GenericSourceBase
        {
            private readonly Type[] _supportedScopeFactoryTypes = {
                                                                      typeof (IScope<>),
                                                                      typeof (IScope<,>),
                                                                      typeof (IScope<,,>)
                                                                  };

            protected override Type[] GetSupportedTypes()
            {
                return _supportedScopeFactoryTypes;
            }

// ReSharper disable UnusedMember.Local
            /// <remarks>
            /// This method is indirectly accessed by <see cref="GenericSourceBase"/>.
            /// </remarks>>
            private static IScope<TRepository> CreateComponent<TRepository>(IComponentContext context) where TRepository : class
// ReSharper restore UnusedMember.Local
            {
                var ss = context.Resolve<ScopeFactory<TRepository>>();
                return ss();
            }

// ReSharper disable UnusedMember.Local
            /// <remarks>
            /// This method is indirectly accessed by <see cref="GenericSourceBase"/>.
            /// </remarks>>
            private static IScope<TRepository1, TRepository2> CreateComponent<TRepository1, TRepository2>(IComponentContext context) 
                where TRepository1 : class
                where TRepository2 : class
// ReSharper restore UnusedMember.Local
            {
                var ss = context.Resolve<ScopeFactory<TRepository1, TRepository2>>();
                return ss();
            }
            
// ReSharper disable UnusedMember.Local
            /// <remarks>
            /// This method is indirectly accessed by <see cref="GenericSourceBase"/>.
            /// </remarks>>
            private static IScope<TRepository1, TRepository2, TRepository3> CreateComponent<TRepository1, TRepository2, TRepository3>(IComponentContext context) 
                where TRepository1 : class
                where TRepository2 : class
                where TRepository3 : class
// ReSharper restore UnusedMember.Local
            {
                var ss = context.Resolve<ScopeFactory<TRepository1, TRepository2, TRepository3>>();
                return ss();
            }

        }

    }
}