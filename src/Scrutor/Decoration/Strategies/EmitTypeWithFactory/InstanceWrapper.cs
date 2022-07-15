namespace Scrutor.Decoration.Strategies.EmitTypeWithFactory
{
    internal class InstanceWrapper
    {
        public object Instance { get; }

        public InstanceWrapper(object instance) => Instance = instance;    
    }
}
