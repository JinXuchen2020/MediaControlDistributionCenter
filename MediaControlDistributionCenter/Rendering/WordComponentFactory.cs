using MediaControlDistributionCenter.ViewModels;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class WordComponentFactory : IComponentFactory
    {
        public string Type => "Word";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is WordComponentViewModel wordVm)
                return new WordPdfRenderable(wordVm);

            throw new ArgumentException($"Expected WordComponentViewModel, got {vm.GetType().Name}");
        }
    }
}
