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
        private readonly BitmapCache? _cache;
        private readonly string _filePath;

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
            _cache = cache;
            _filePath = filePath ?? string.Empty;
            ZIndex = vm.ZIndex;
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    _bitmap = cache?.GetOrDecode(filePath) ?? SKBitmap.Decode(filePath);
                }
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
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }

        public void Dispose()
        {
            if (_cache != null && !string.IsNullOrEmpty(_filePath))
            {
                _cache.Release(_filePath);
                _bitmap = null;
            }
            else
            {
                _bitmap?.Dispose();
                _bitmap = null;
            }
        }
    }
}
