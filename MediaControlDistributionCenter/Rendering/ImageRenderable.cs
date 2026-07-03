using MediaControlDistributionCenter.ViewModels;
using Serilog;
using SkiaSharp;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Rendering
{
    public class ImageRenderable : IRenderable
    {
        private SKBitmap? _bitmap;
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;
        private readonly BitmapCache? _cache = null;
        private string? _filePath;
        private Task<SKBitmap?>? _decodeTask;

        public string Type => "Image";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public BaseComponentViewModel? ViewModel => _vm;

        internal static volatile int PendingDecodeCount;

        public event Action<IRenderable>? Invalidated;

        public ImageRenderable(BaseComponentViewModel vm, string filePath) : this(vm, filePath, null)
        {
        }

        public ImageRenderable(BaseComponentViewModel vm, string filePath, BitmapCache? cache)
        {
            _vm = vm;
            _filePath = filePath;
            ZIndex = vm.ZIndex;
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    _bitmap = cache?.GetOrDecode(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decode image: {FilePath}", filePath);
            }
            if (_bitmap == null && !string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Interlocked.Increment(ref PendingDecodeCount);
                _decodeTask = Task.Run(() =>
                {
                    try { return SKBitmap.Decode(_filePath); }
                    catch (Exception ex) { Log.Error(ex, "Async decode failed: {FilePath}", _filePath); return null; }
                });
            }
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            if (_decodeTask != null && _bitmap == null)
            {
                if (_decodeTask.IsCompleted)
                {
                    _bitmap = _decodeTask.Result;
                    _decodeTask = null;
                    Interlocked.Decrement(ref PendingDecodeCount);
                    Invalidated?.Invoke(this);
                }
                else
                {
                    return;
                }
            }

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
            Invalidated?.Invoke(this);
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
            _decodeTask = null;
        }
    }
}
