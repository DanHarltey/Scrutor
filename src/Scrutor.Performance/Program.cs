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
            var summary = BenchmarkRunner.Run(typeof(Program));
        }

        private ServiceProvider? proposedNextServiceProvider;
        private ServiceProvider? proposedServiceProvider;
        private ServiceProvider? currentServiceProvider;
        private ServiceProvider? oldServiceProvider;


        [GlobalSetup]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddTransient<IService, SomeRandomService>()
                .AddTransient<IDecoratedService, Decorated>()
                .DecorateNext(typeof(IDecoratedService), typeof(Decorator));


            proposedNextServiceProvider = services.BuildServiceProvider();

            services = new ServiceCollection();

            services
                .AddTransient<IService, SomeRandomService>()
                .AddTransient<IDecoratedService, Decorated>()
                .Decorate<IDecoratedService, Decorator>();


            proposedServiceProvider = services.BuildServiceProvider();

            services = new ServiceCollection();

            services
                .AddTransient<IService, SomeRandomService>()
                .AddTransient<IDecoratedService, Decorated>()
                .DecorateCurrent(typeof(IDecoratedService), typeof(Decorator));


            currentServiceProvider = services.BuildServiceProvider();

            services = new ServiceCollection();

            services
                .AddTransient<IService, SomeRandomService>()
                .AddTransient<IDecoratedService, Decorated>()
                .DecorateV4(typeof(IDecoratedService), typeof(Decorator));


            oldServiceProvider = services.BuildServiceProvider();
        }

        [Benchmark()]
        public object FuturePr() => proposedNextServiceProvider!.GetRequiredService<IDecoratedService>();

        [Benchmark(Baseline = true)]
        public object PrProposed() => proposedServiceProvider!.GetRequiredService<IDecoratedService>();

        [Benchmark]
        public object DecorateV42() => currentServiceProvider!.GetRequiredService<IDecoratedService>();

        [Benchmark]
        public object DecorateV41() => oldServiceProvider!.GetRequiredService<IDecoratedService>();

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
