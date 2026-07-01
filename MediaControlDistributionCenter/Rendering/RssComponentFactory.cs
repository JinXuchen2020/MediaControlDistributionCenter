using MediaControlDistributionCenter.ViewModels;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class RssComponentFactory : IComponentFactory
    {
        public string Type => "Rss";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is RssComponentViewModel rssVm)
                return new RssRenderable(rssVm);

            throw new ArgumentException($"Expected RssComponentViewModel, got {vm.GetType().Name}");
        }
    }
}
