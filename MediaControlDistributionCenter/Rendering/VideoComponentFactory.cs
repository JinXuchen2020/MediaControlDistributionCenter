using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class VideoComponentFactory : IComponentFactory
    {
        public string Type => "Video";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            return new VideoRenderable(vm);
        }
    }
}
