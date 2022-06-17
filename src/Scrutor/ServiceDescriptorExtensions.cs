using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceDescriptorExtensions
{
    public static ServiceDescriptor WithImplementationFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> implementationFactory) => 
        new(descriptor.ServiceType, implementationFactory, descriptor.Lifetime);

    public static ServiceDescriptor WithServiceType(this ServiceDescriptor descriptor, Type serviceType) => descriptor switch
    {
        { ImplementationType: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationType, descriptor.Lifetime),
        { ImplementationFactory: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime),
        { ImplementationInstance: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };

    public static ServiceDescriptor WithDecoratedType(this ServiceDescriptor descriptor, DecoratedType serviceType) => descriptor switch
    {
        { ImplementationType: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationType.ToFactory(), descriptor.Lifetime),
        { ImplementationFactory: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime),
        { ImplementationInstance: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };

    public static ServiceDescriptor WithDecoratedTypeNext(this ServiceDescriptor descriptor, DecoratedType serviceType) => descriptor switch
    {
        { ImplementationType: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationType.ToFactory2(), descriptor.Lifetime),
        { ImplementationFactory: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime),
        { ImplementationInstance: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };

    private static Func<IServiceProvider, object> ToFactory(this Type serviceType) => 
        serviceProvider => ActivatorUtilities.CreateInstance(serviceProvider, serviceType);

    private static Func<IServiceProvider, object> ToFactory2(this Type serviceType)
    {
        return new Cache(serviceType).Create;
    }

    // The Cache should be registered as a singleton, so it that it can
    // act as a cache for the Activator. This allows the outer class to be registered
    // as a transient, so that it doesn't close over the application root service provider.
    public class Cache
    {
        private readonly Func<ObjectFactory> _createActivator;

        private ObjectFactory? _activator;
        private bool _initialized;
        private object? _lock;

        public Cache(Type type)
        {
            _createActivator = () => ActivatorUtilities.CreateFactory(type, Array.Empty<Type>());
        }

        public ObjectFactory Activator => LazyInitializer.EnsureInitialized(
            ref _activator,
            ref _initialized,
            ref _lock!,
            _createActivator)!;

        public object Create(IServiceProvider services) => Activator(services, Array.Empty<object>());
    }
}
