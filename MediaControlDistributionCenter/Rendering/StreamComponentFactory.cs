using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class StreamComponentFactory : IComponentFactory
    {
        public string Type => "Stream";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            return new StreamRenderable(vm);
        }
    }
}
