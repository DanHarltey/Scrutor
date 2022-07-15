using System;

namespace Scrutor.Decoration
{
    internal class DecorationFactory : IDecorationFactory
    {
        public IDecoration Create(Type serviceType, Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory)
        {
            IDecorationStrategy strategy;

            if (serviceType.IsOpenGeneric())
            {
                strategy = new OpenGenericDecorationStrategy(serviceType, decoratorType, decoratorFactory);
            }
            else
            {
                strategy = new ClosedTypeDecorationStrategy(serviceType, decoratorType, decoratorFactory);
            }

            return new Decoration(strategy);
        }

        public IDecoration Create<TService>(Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory)
            => new Decoration(new ClosedTypeDecorationStrategy(typeof(TService), decoratorType, decoratorFactory));
    }
}
