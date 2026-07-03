using MediaControlDistributionCenter.ViewModels;
using Serilog;
using SkiaSharp;
using System.Collections.Generic;
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
        public IReadOnlyList<IRenderable>? Children => null;

        public bool IsDecoding => _decodeTask != null && !_decodeTask.IsCompleted;

        internal static volatile int PendingDecodeCount;

        public event Action<IRenderable, SKRect>? Invalidated;

        public ImageRenderable(BaseComponentViewModel vm, string filePath) : this(vm, filePath, null)
        {
        }

        public ImageRenderable(BaseComponentViewModel vm, string filePath, BitmapCache? cache)
        {
            _vm = vm;
            _filePath = filePath;
            ZIndex = vm.ZIndex;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                if (cache != null)
                {
                    Interlocked.Increment(ref PendingDecodeCount);
                    _decodeTask = cache.GetOrDecodeAsync(filePath).ContinueWith(t =>
                    {
                        try { return t.Result; }
                        catch (Exception ex) { Log.Error(ex, "Cache decode failed: {FilePath}", _filePath); return null; }
                    });
                }
                else
                {
                    Interlocked.Increment(ref PendingDecodeCount);
                    _decodeTask = Task.Run(() =>
                    {
                        try
                        {
                            using var stream = File.OpenRead(_filePath);
                            return SKBitmap.Decode(stream);
                        }
                        catch (Exception ex) { Log.Error(ex, "Async decode failed: {FilePath}", _filePath); return null; }
                    });
                }
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
                    Invalidated?.Invoke(this, Bounds);
                }
                else
                {
                    // loading placeholder
                    var bg = RenderResourcePool.Shared.RentPaint();
                    bg.Color = new SKColor(60, 60, 60, 180);
                    bg.Style = SKPaintStyle.Fill;
                    canvas.DrawRect(_bounds, bg);
                    RenderResourcePool.Shared.ReturnPaint(bg);
                    
                    var font = RenderResourcePool.Shared.RentFont(14f);
                    font.Typeface = RenderResourcePool.BoldTypeface;
                    string label = "Loading...";
                    float tw = font.MeasureText(label);
                    float lx = _bounds.MidX - tw / 2;
                    float ly = _bounds.MidY + 5;
                    var textPaint = RenderResourcePool.Shared.RentPaint();
                    textPaint.Color = new SKColor(200, 200, 200, 255);
                    canvas.DrawText(label, lx, ly, SKTextAlign.Left, font, textPaint);
                    RenderResourcePool.Shared.ReturnPaint(textPaint);
                    RenderResourcePool.Shared.ReturnFont(font);
                    return;
                }
            }

            if (_bitmap == null) return;

            var paint = RenderResourcePool.Shared.RentPaint();
            canvas.DrawBitmap(_bitmap, _bounds, new SKSamplingOptions(), paint);
            RenderResourcePool.Shared.ReturnPaint(paint);
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            UpdateBounds();
            Invalidated?.Invoke(this, Bounds);
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
