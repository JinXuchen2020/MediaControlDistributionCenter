using MediaControlDistributionCenter.ViewModels;
using Serilog;
using SkiaSharp;
using System.IO;

namespace MediaControlDistributionCenter.Rendering
{
    public class ImageRenderable : IRenderable
    {
        private SKBitmap? _bitmap;
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;

        public string Type => "Image";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public BaseComponentViewModel? ViewModel => _vm;

        public ImageRenderable(BaseComponentViewModel vm, string filePath) : this(vm, filePath, null)
        {
        }

        public ImageRenderable(BaseComponentViewModel vm, string filePath, BitmapCache? cache)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    _bitmap = cache?.GetOrDecode(filePath) ?? SKBitmap.Decode(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decode image: {FilePath}", filePath);
            }
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            if (_bitmap == null) return;

            var paint = RenderResourcePool.Shared.RentPaint();
            canvas.DrawBitmap(_bitmap, _bounds, paint);
            RenderResourcePool.Shared.ReturnPaint(paint);
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            _bounds = new SKRect(
                (float)(_vm.Left * _vm.Ratio),
                (float)(_vm.Top * _vm.Ratio),
                (float)((_vm.Left + _vm.Width) * _vm.Ratio),
                (float)((_vm.Top + _vm.Height) * _vm.Ratio));
        }

        public void Dispose()
        {
            _bitmap?.Dispose();
        }
    }
}
