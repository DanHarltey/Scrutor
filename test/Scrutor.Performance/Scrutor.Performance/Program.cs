using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor.Performance
{
    [MemoryDiagnoser]
    public class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run(typeof(Program));
        }

        private ServiceProvider? originalStrategyProvider;
        private ServiceProvider? proxiedTypeProvider;
        private ServiceProvider? emitTypeWithFactoryProvider;
        private ServiceProvider? emitTypeCtor;

        [GlobalSetup]
        public void SetUp()
        {
            ServiceCollectionExtensions.DecorationFactory = new Decoration.Strategies.Original.DecorationFactory();
            originalStrategyProvider = CreateServiceProvider();

            ServiceCollectionExtensions.DecorationFactory = new Decoration.Strategies.ProxiedType.DecorationFactory();
            proxiedTypeProvider = CreateServiceProvider();

            ServiceCollectionExtensions.DecorationFactory = new Decoration.Strategies.EmitTypeWithFactory.DecorationFactory();
            emitTypeWithFactoryProvider = CreateServiceProvider();

            ServiceCollectionExtensions.DecorationFactory = new Decoration.Strategies.EmitTypeCtor.DecorationFactory();
            emitTypeCtor = CreateServiceProvider();
        }

        private static ServiceProvider CreateServiceProvider() => 
            new ServiceCollection()
            .AddTransient<IService, SomeRandomService>()
            .AddTransient<IDecoratedService, Decorated>()
            .Decorate<IDecoratedService, Decorator>()
            .BuildServiceProvider(); 

        [Benchmark(Baseline = true)]
        public object OriginalStrategy() => originalStrategyProvider!.GetRequiredService<IDecoratedService>();

        [Benchmark]
        public object ProxiedTypeStrategy() => proxiedTypeProvider!.GetRequiredService<IDecoratedService>();

        [Benchmark]
        public object EmitTypeWithFactoryStrategy() => emitTypeWithFactoryProvider!.GetRequiredService<IDecoratedService>();

        [Benchmark]
        public object EmitTypeWithCtor() => emitTypeCtor!.GetRequiredService<IDecoratedService>();

        public interface IDecoratedService { }

        public interface IService { }

        private class SomeRandomService : IService { }

        public class Decorated : IDecoratedService
        {
            public Decorated(IService injectedService)
            {
                InjectedService = injectedService;
            }

            public IService InjectedService { get; }
        }

        public class Decorator : IDecoratedService
        {
            public Decorator(IDecoratedService inner, IService injectedService)
            {
                Inner = inner;
                InjectedService = injectedService;
            }

            public IDecoratedService Inner { get; }

            public IService InjectedService { get; }
        }
    }
}
