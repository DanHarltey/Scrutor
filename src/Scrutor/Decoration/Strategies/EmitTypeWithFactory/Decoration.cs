using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scrutor.Decoration.Strategies.EmitTypeWithFactory
{
    internal sealed class Decoration : IDecoration
    {
        private readonly IDecorationStrategy _decorationStrategy;

        public Decoration(IDecorationStrategy decorationStrategy)
            => _decorationStrategy = decorationStrategy;

        public bool TryDecorate(IServiceCollection services)
        {
            var decorated = DecorateServices(services);
            return decorated != 0;
        }

        public IServiceCollection Decorate(IServiceCollection services)
        {
            var decorated = DecorateServices(services);

            if (decorated == 0)
            {
                throw new MissingTypeRegistrationException(_decorationStrategy.ServiceType);
            }

            return services;
        }

        private int DecorateServices(IServiceCollection services)
        {
            int decorated = 0;

            for (int i = services.Count - 1; i >= 0; i--)
            {
                var serviceDescriptor = services[i];

                if (IsNotAlreadyDecorated(serviceDescriptor)
                    && _decorationStrategy.CanDecorate(serviceDescriptor.ServiceType))
                {
                    var decoratedType = ProxyTypeFactory.CreateWrapperType(serviceDescriptor.ServiceType);

                    var decoratorFactory = _decorationStrategy.CreateDecorator(decoratedType);

                    // insert decorated
                    var decoratedServiceDescriptor = CreateDecoratedServiceDescriptor(serviceDescriptor, decoratedType);
                    services.Add(decoratedServiceDescriptor);

                    // replace decorator
                    services[i] = new ServiceDescriptor(serviceDescriptor.ServiceType, decoratorFactory, serviceDescriptor.Lifetime);

                    ++decorated;
                }
            }

            return decorated;
        }

        private static bool IsNotAlreadyDecorated(ServiceDescriptor serviceDescriptor) => !typeof(InstanceWrapper).IsAssignableFrom(serviceDescriptor.ServiceType);

        private static ServiceDescriptor CreateDecoratedServiceDescriptor(ServiceDescriptor serviceDescriptor, Type decoratedType) => serviceDescriptor switch
        {
            { ImplementationType: not null } => new ServiceDescriptor(decoratedType, ImplementationTypeToFactory(serviceDescriptor.ImplementationType, decoratedType), serviceDescriptor.Lifetime),
            { ImplementationFactory: not null } => new ServiceDescriptor(decoratedType, ImplementationFactoryToFactory(serviceDescriptor.ImplementationFactory, decoratedType), serviceDescriptor.Lifetime),
            { ImplementationInstance: not null } => new ServiceDescriptor(decoratedType, ImplementationInstanceToInstance(serviceDescriptor.ImplementationInstance, decoratedType)),
            _ => throw new ArgumentException($"No implementation factory or instance or type found for {serviceDescriptor.ServiceType}.", nameof(serviceDescriptor))
        };

        private static Func<IServiceProvider, object> ImplementationTypeToFactory(Type implementationType, Type decoratedWrapper)
        {
            var ctor = decoratedWrapper.GetConstructor(new[] { typeof(object) })!;
            return (serviceProvider) =>
            {
                var instanceToDecorate = ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
                return ctor.Invoke(new[] { instanceToDecorate });
            };
        }

        private static Func<IServiceProvider, object> ImplementationFactoryToFactory(Func<IServiceProvider, object> factory, Type decoratedWrapper)
        {
            var ctor = decoratedWrapper.GetConstructor(new[] { typeof(object) })!;
            return (serviceProvider) =>
            {
                var instanceToDecorate = factory(serviceProvider);
                return ctor.Invoke(new[] { instanceToDecorate });
            };
        }

        private static object ImplementationInstanceToInstance(object instance, Type decoratedWrapper)
        {
            var ctor = decoratedWrapper.GetConstructor(new[] { typeof(object) })!;
            return ctor.Invoke(new[] { instance });
        }
    }
}
