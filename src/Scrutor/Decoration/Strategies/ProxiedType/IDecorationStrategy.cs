﻿using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scrutor.Decoration.Strategies.ProxiedType
{
    internal interface IDecorationStrategy
    {
        public Type ServiceType { get; }
        public bool CanDecorate(Type serviceType);
        public Func<IServiceProvider, object> CreateDecorator(Type serviceType);
    }
}
