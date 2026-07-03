using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class RenderableRegistry
    {
        private readonly Dictionary<string, IComponentFactory> _factories = new();
        private readonly AnimationEngine? _animationEngine;

        public RenderableRegistry(AnimationEngine? animationEngine = null)
        {
            _animationEngine = animationEngine;
        }

        internal AnimationEngine? AnimationEngine => _animationEngine;

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

    public static class RenderableRegistryExtensions
    {
        public static void RegisterDefaultFactories(this RenderableRegistry registry, BitmapCache? cache = null)
        {
            registry.Register(new ImageComponentFactory(cache));
            registry.Register(new ColorTextComponentFactory());
            registry.Register(new TextComponentFactory(registry.AnimationEngine));
            registry.Register(new RssComponentFactory());
            registry.Register(new WordComponentFactory());
            registry.Register(new VideoComponentFactory());
            registry.Register(new WebComponentFactory());
            registry.Register(new StreamComponentFactory());
            registry.Register(new HdmiComponentFactory());
        }
    }
}
