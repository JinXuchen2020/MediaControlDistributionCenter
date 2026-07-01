using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class RenderableRegistry
    {
        private readonly Dictionary<string, IComponentFactory> _factories = new();

        public void Register(IComponentFactory factory)
        {
            _factories[factory.Type] = factory;
        }

        public IRenderable Create(BaseComponentViewModel vm)
        {
            if (_factories.TryGetValue(vm.Type, out var factory))
            {
                var renderable = factory.Create(vm);
                return renderable ?? throw new InvalidOperationException($"Factory for '{vm.Type}' returned null");
            }
            throw new KeyNotFoundException($"No factory registered for component type: {vm.Type}");
        }

        public bool CanCreate(string type) => _factories.ContainsKey(type);
    }
}
