using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public interface IComponentFactory
    {
        IRenderable Create(BaseComponentViewModel vm);
        string Type { get; }
        IRenderable Create(BaseComponentViewModel vm, AnimationEngine? animationEngine) => Create(vm);
    }
}
