using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public static class TextComponentFactory
    {
        public static TextRenderable Create(BaseComponentViewModel vm)
        {
            if (vm is TextComponentViewModel textVm)
                return new TextRenderable(textVm);

            return null;
        }
    }
}
