using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class HdmiComponentFactory : IComponentFactory
    {
        public string Type => "Hdmi";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            return new HdmiRenderable(vm);
        }
    }
}
