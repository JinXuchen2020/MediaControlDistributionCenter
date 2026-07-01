using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class WebComponentFactory : IComponentFactory
    {
        public string Type => "Web";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            return new WebRenderable(vm);
        }
    }
}
