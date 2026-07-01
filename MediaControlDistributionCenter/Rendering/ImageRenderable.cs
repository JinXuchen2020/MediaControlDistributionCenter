using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

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

        public ImageRenderable(BaseComponentViewModel vm, string filePath)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    _bitmap = SKBitmap.Decode(filePath);
            }
            catch { }
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            if (_bitmap == null) return;

            using var paint = new SKPaint
            {
                IsAntialias = true,
            };

            canvas.DrawBitmap(_bitmap, _bounds, paint);
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
