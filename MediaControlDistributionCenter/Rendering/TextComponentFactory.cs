using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class TextComponentFactory : IComponentFactory
    {
        public string Type => "Text";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is TextComponentViewModel textVm)
                return new TextRenderable(textVm);
            throw new ArgumentException($"Expected TextComponentViewModel, got {vm.GetType().Name}");
        }
    }
}
