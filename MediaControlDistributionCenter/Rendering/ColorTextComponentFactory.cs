using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class ColorTextComponentFactory : IComponentFactory
    {
        public string Type => "ColorText";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is ColorTextComponentViewModel colorVm)
                return new ColorTextRenderable(colorVm);

            return new ColorTextRenderable(vm as ColorTextComponentViewModel
                ?? throw new ArgumentException("Expected ColorTextComponentViewModel"));
        }
    }
}
