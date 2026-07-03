using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class TextComponentFactory : IComponentFactory
    {
        private readonly AnimationEngine? _animationEngine;

        public TextComponentFactory(AnimationEngine? animationEngine = null)
        {
            _animationEngine = animationEngine;
        }

        public string Type => "Text";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            return Create(vm, _animationEngine);
        }

        public IRenderable Create(BaseComponentViewModel vm, AnimationEngine? animationEngine)
        {
            if (vm is TextComponentViewModel textVm)
                return new TextRenderable(textVm, animationEngine);
            throw new ArgumentException($"Expected TextComponentViewModel, got {vm.GetType().Name}");
        }
    }
}
