using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    internal static class BoundsHelper
    {
        public static SKRect ComputeBounds(BaseComponentViewModel vm)
        {
            return new SKRect(
                (float)(vm.Left * vm.Ratio * vm.CanvasRatio),
                (float)(vm.Top * vm.Ratio * vm.CanvasRatio),
                (float)((vm.Left + vm.Width) * vm.Ratio * vm.CanvasRatio),
                (float)((vm.Top + vm.Height) * vm.Ratio * vm.CanvasRatio));
        }
    }
}
