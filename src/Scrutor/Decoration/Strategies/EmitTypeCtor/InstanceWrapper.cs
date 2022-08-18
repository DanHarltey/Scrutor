using System;
using System.Threading.Tasks;

namespace Scrutor.Decoration.Strategies.EmitTypeCtor
{
    public class InstanceWrapper// : IDisposable, IAsyncDisposable
    {
        public object Instance { get; }

        public InstanceWrapper(object instance) => Instance = instance;

        public void Dispose() => (Instance as IDisposable)?.Dispose();

        public ValueTask DisposeAsync() => Instance is IAsyncDisposable disposable ? disposable.DisposeAsync() : new ValueTask();
    }
}
