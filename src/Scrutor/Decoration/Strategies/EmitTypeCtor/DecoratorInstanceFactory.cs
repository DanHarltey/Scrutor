using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scrutor.Decoration.Strategies.EmitTypeCtor
{
    internal static class DecoratorInstanceFactory
    {
        internal static Func<IServiceProvider, object> Default(Type decorated, Type decorator) =>
            (serviceProvider) =>
            {
                var wrapperOfInstanceToDecorate = (InstanceWrapper)serviceProvider.GetRequiredService(decorated);
                return ActivatorUtilities.CreateInstance(serviceProvider, decorator, wrapperOfInstanceToDecorate.Instance);
            };

        internal static Func<IServiceProvider, object> Custom(Type decorated, Func<object, IServiceProvider, object> creationFactory) =>
            (serviceProvider) =>
            {
                var wrapperOfInstanceToDecorate = (InstanceWrapper)serviceProvider.GetRequiredService(decorated);
                return creationFactory(wrapperOfInstanceToDecorate.Instance, serviceProvider);
            };
    }
}
