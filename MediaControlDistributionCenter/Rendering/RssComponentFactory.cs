using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public static class RssComponentFactory
    {
        public static RssRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is RssComponentViewModel rssVm)
                return new RssRenderable(rssVm);

            return null;
        }
    }
}
