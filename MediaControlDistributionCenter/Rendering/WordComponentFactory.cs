using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public static class WordComponentFactory
    {
        public static WordPdfRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is WordComponentViewModel wordVm)
                return new WordPdfRenderable(wordVm);

            return null;
        }
    }
}
