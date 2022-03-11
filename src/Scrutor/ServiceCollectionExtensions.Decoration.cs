using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Scrutor;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Decorates all registered services of type <typeparamref name="TService"/>
        /// using the specified type <typeparamref name="TDecorator"/>.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <exception cref="MissingTypeRegistrationException">If no service of the type <typeparamref name="TService"/> has been registered.</exception>
        /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
        public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
            where TDecorator : TService
        {
            Preconditions.NotNull(services, nameof(services));

            return _DecorateDescriptors(services, typeof(TService), _CreateInstanceFactory(typeof(TDecorator)));
        }

        /// <summary>
        /// Decorates all registered services of type <typeparamref name="TService"/>
        /// using the specified type <typeparamref name="TDecorator"/>.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
        public static bool TryDecorate<TService, TDecorator>(this IServiceCollection services)
            where TDecorator : TService
        {
            Preconditions.NotNull(services, nameof(services));

            var decorated = _TryDecorate(services, _CreateTypeSelector(typeof(TService)), _CreateInstanceFactory(typeof(TDecorator)));

            return decorated != 0;
        }

        /// <summary>
        /// Decorates all registered services of the specified <paramref name="serviceType"/>
        /// using the specified <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <param name="serviceType">The type of services to decorate.</param>
        /// <param name="decoratorType">The type to decorate existing services with.</param>
        /// <exception cref="MissingTypeRegistrationException">If no service of the specified <paramref name="serviceType"/> has been registered.</exception>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
        /// <paramref name="serviceType"/> or <paramref name="decoratorType"/> arguments are <c>null</c>.</exception>
        public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType)
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(serviceType, nameof(serviceType));
            Preconditions.NotNull(decoratorType, nameof(decoratorType));

            if (serviceType.IsOpenGeneric() && decoratorType.IsOpenGeneric())
            {
                return _DecorateOpenGeneric(services, serviceType, _CreateInstanceFactory(decoratorType));
            }

            return _DecorateDescriptors(services, serviceType, _CreateInstanceFactory(decoratorType));
        }

        /// <summary>
        /// Decorates all registered services of the specified <paramref name="serviceType"/>
        /// using the specified <paramref name="decoratorType"/>.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <param name="serviceType">The type of services to decorate.</param>
        /// <param name="decoratorType">The type to decorate existing services with.</param>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
        /// <paramref name="serviceType"/> or <paramref name="decoratorType"/> arguments are <c>null</c>.</exception>
        public static bool TryDecorate(this IServiceCollection services, Type serviceType, Type decoratorType)
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(serviceType, nameof(serviceType));
            Preconditions.NotNull(decoratorType, nameof(decoratorType));

            Func<Type, bool> selector;

            if (serviceType.IsOpenGeneric() && decoratorType.IsOpenGeneric())
            {
                selector = _CreateOpenGenericSelector(serviceType);
            }
            else
            {
                selector = _CreateTypeSelector(serviceType);
            }

            var decorated = _TryDecorate(services, selector, _CreateInstanceFactory(decoratorType));

            return decorated != 0;
        }

        /// <summary>
        /// Decorates all registered services of type <typeparamref name="TService"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <typeparam name="TService">The type of services to decorate.</typeparam>
        /// <param name="services">The services to add to.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="MissingTypeRegistrationException">If no service of <typeparamref name="TService"/> has been registered.</exception>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
        /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator) where TService : notnull
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(decorator, nameof(decorator));
            throw new NotImplementedException();
            //return _DecorateDescriptors(services, typeof(TService), decorator);
        }

        /// <summary>
        /// Decorates all registered services of type <typeparamref name="TService"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <typeparam name="TService">The type of services to decorate.</typeparam>
        /// <param name="services">The services to add to.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
        /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static bool TryDecorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator) where TService : notnull
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(decorator, nameof(decorator));
            throw new NotImplementedException();

            //return services._TryDecorateDescriptors(typeof(TService), out _, x => x.Decorate(decorator));
        }

        /// <summary>
        /// Decorates all registered services of type <typeparamref name="TService"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <typeparam name="TService">The type of services to decorate.</typeparam>
        /// <param name="services">The services to add to.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="MissingTypeRegistrationException">If no service of <typeparamref name="TService"/> has been registered.</exception>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
        /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, TService> decorator) where TService : notnull
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(decorator, nameof(decorator));
            throw new NotImplementedException();

            //return services.DecorateDescriptors(typeof(TService), x => x.Decorate(decorator));
        }

        /// <summary>
        /// Decorates all registered services of type <typeparamref name="TService"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <typeparam name="TService">The type of services to decorate.</typeparam>
        /// <param name="services">The services to add to.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
        /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static bool TryDecorate<TService>(this IServiceCollection services, Func<TService, TService> decorator) where TService : notnull
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(decorator, nameof(decorator));
            throw new NotImplementedException();

            //return services._TryDecorateDescriptors(typeof(TService), out _, x => x.Decorate(decorator));
        }

        /// <summary>
        /// Decorates all registered services of the specified <paramref name="serviceType"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <param name="serviceType">The type of services to decorate.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="MissingTypeRegistrationException">If no service of the specified <paramref name="serviceType"/> has been registered.</exception>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
        /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(serviceType, nameof(serviceType));
            Preconditions.NotNull(decorator, nameof(decorator));

            // why no open Generic
            return _DecorateDescriptors(services, serviceType, decorator);
        }

        /// <summary>
        /// Decorates all registered services of the specified <paramref name="serviceType"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <param name="serviceType">The type of services to decorate.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
        /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(serviceType, nameof(serviceType));
            Preconditions.NotNull(decorator, nameof(decorator));

            // why no open Generic
            return 0 != _TryDecorate(services, _CreateTypeSelector(serviceType), decorator);
        }

        /// <summary>
        /// Decorates all registered services of the specified <paramref name="serviceType"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <param name="serviceType">The type of services to decorate.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="MissingTypeRegistrationException">If no service of the specified <paramref name="serviceType"/> has been registered.</exception>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
        /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(serviceType, nameof(serviceType));
            Preconditions.NotNull(decorator, nameof(decorator));

            // why no open Generic
            return _DecorateDescriptors(services, serviceType, (instanceToDecorate, sp) => decorator(instanceToDecorate));
        }

        /// <summary>
        /// Decorates all registered services of the specified <paramref name="serviceType"/>
        /// using the <paramref name="decorator"/> function.
        /// </summary>
        /// <param name="services">The services to add to.</param>
        /// <param name="serviceType">The type of services to decorate.</param>
        /// <param name="decorator">The decorator function.</param>
        /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
        /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
        public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
        {
            Preconditions.NotNull(services, nameof(services));
            Preconditions.NotNull(serviceType, nameof(serviceType));
            Preconditions.NotNull(decorator, nameof(decorator));

            return 0 != _TryDecorate(services, _CreateTypeSelector(serviceType), (instanceToDecorate, sp) => decorator(instanceToDecorate));
        }

        // ---------------------

        private static IServiceCollection _DecorateDescriptors(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> creationFactory)
        {
            var decorated = _TryDecorate(services, _CreateTypeSelector(serviceType), creationFactory);

            if (decorated == 0)
            {
                throw new MissingTypeRegistrationException(serviceType);
            }

            return services;
        }

        private static IServiceCollection _DecorateOpenGeneric(IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> creationFactory)
        {
            var decorated = _TryDecorate(services, _CreateOpenGenericSelector(serviceType), creationFactory);

            if (decorated == 0)
            {
                throw new MissingTypeRegistrationException(serviceType);
            }

            return services;
        }

        private static int _TryDecorate(IServiceCollection services, Func<Type, bool> selector, Func<object, IServiceProvider, object> creationFactory)
        {
            int decorated = 0;

            for (int i = services.Count - 1; i >= 0; i--)
            {
                var service = services[i];

                if (selector(service.ServiceType))
                {
                    var decoratedType = new DecoratedType(service.ServiceType);

                    // insert decorator
                    services[i] = new ServiceDescriptor(service.ServiceType, _CreateImplementationFactory(decoratedType, creationFactory), service.Lifetime);

                    // insert decorated
                    var decoratedTypeDescriptor = _DecorateServiceDescriptor(service, decoratedType);
                    services.Add(decoratedTypeDescriptor);

                    ++decorated;
                }
            }

            return decorated;
        }

        ////private ServiceDescriptor _Decorate(IServiceCollection services, int index, ServiceDescriptor service, Type decoratorType, Func<object, IServiceProvider, object> creationFactory)
        ////{
        ////    var decoratedType = new DecoratedType(service.ServiceType);

        ////    // insert decorator
        ////    services[index] = new ServiceDescriptor(service.ServiceType, _CreateImplementationFactory(decoratedType, creationFactory), service.Lifetime);

        ////    // insert decorated
        ////    var decoratedTypeDescriptor = _DecorateServiceDescriptor(service, decoratedType);
        ////    services.Add(decoratedTypeDescriptor);
        ////}

        ////private static int _____TryDecorateOpenGeneric(IServiceCollection services, Func<Type, bool> selector, Type serviceType, Type decoratorType, Func<object, IServiceProvider, object> creationFactory)
        ////{
        ////    int decorated = 0;

        ////    for (int i = services.Count - 1; i >= 0; i--)
        ////    {
        ////        var service = services[i];

        ////        if (selector(service.ServiceType))
        ////        {
        ////            var arguments = service.ServiceType.GetGenericArguments();

        ////            var closedServiceType = serviceType.MakeGenericType(arguments);
        ////            try
        ////            {
        ////                var closedDecoratorType = decoratorType.MakeGenericType(arguments);
       
        ////            }
        ////            catch (ArgumentException) { }
        ////        }
        ////    }

        ////    return decorated;
        ////}

        private static ServiceDescriptor _DecorateServiceDescriptor(ServiceDescriptor descriptor, Type decoratedType) => descriptor switch
        {
            { ImplementationType: not null } => new ServiceDescriptor(decoratedType, descriptor.ImplementationType, descriptor.Lifetime),
            { ImplementationFactory: not null } => new ServiceDescriptor(decoratedType, descriptor.ImplementationFactory, descriptor.Lifetime),
            { ImplementationInstance: not null } => new ServiceDescriptor(decoratedType, descriptor.ImplementationInstance),
            _ => throw new InvalidOperationException($"No implementation factory or instance or type found for {descriptor.ServiceType}.")
        };

        private static Func<IServiceProvider, object> _CreateImplementationFactory(Type decorated, Func<object, IServiceProvider, object> creationFactory) => (serviceProvider) =>
        {
            var instanceToDecorate = serviceProvider.GetService(decorated);

            return creationFactory(instanceToDecorate, serviceProvider);
        };

        private static Func<object, IServiceProvider, object> _CreateInstanceFactory(Type decorator) => (instanceToDecorate, serviceProvider)
            => ActivatorUtilities.CreateInstance(serviceProvider, decorator, instanceToDecorate);

        private static Func<object, IServiceProvider, object> _CreateOpenGenericInstanceFactory(Type decorator, Type decorated)
        {
            make this used and tests replace _CreateInstanceFactory where needed
            var arguments = decorated.GetGenericArguments();

            var closedDecorator = decorator.MakeGenericType(arguments);

            return (instanceToDecorate, serviceProvider) => ActivatorUtilities.CreateInstance(serviceProvider, closedDecorator, instanceToDecorate);
        }

        private static Func<Type, bool> _CreateTypeSelector(Type serviceType) => (descriptorType)
            => descriptorType == serviceType;

        private static Func<Type, bool> _CreateOpenGenericSelector(Type serviceType) => (descriptorType)
            => (!descriptorType.IsGenericTypeDefinition) && _HasSameTypeDefinition(descriptorType, serviceType);

        private static bool _HasSameTypeDefinition(Type t1, Type t2)
            => t1.IsGenericType && t2.IsGenericType && t1.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition();
    }
}

