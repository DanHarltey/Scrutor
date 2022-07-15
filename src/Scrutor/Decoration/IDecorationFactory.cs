using System;

namespace Scrutor.Decoration
{
    public interface IDecorationFactory
    {
        IDecoration Create(Type serviceType, Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory);
        IDecoration Create<TService>(Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory);
    }
}
