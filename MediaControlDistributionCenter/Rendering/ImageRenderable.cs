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
        private volatile bool _disposed;
        private int _pdcDecremented;

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
                    _cache = cache;
                    _decodeTask = cache.GetOrDecodeAsync(filePath).ContinueWith(t =>
                    {
                        try
                        {
                            var result = t.Result;
                            if (_disposed)
                            {
                                if (Interlocked.Exchange(ref _pdcDecremented, 1) == 0)
                                    Interlocked.Decrement(ref PendingDecodeCount);
                                return null;
                            }
                            return result;
                        }
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
                            var decoded = SKBitmap.Decode(stream);
                            if (_disposed)
                            {
                                decoded?.Dispose();
                                if (Interlocked.Exchange(ref _pdcDecremented, 1) == 0)
                                    Interlocked.Decrement(ref PendingDecodeCount);
                                return null;
                            }
                            return decoded;
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
                    if (Interlocked.Exchange(ref _pdcDecremented, 1) == 0)
                        Interlocked.Decrement(ref PendingDecodeCount);
                    if (_disposed)
                    {
                        _bitmap = null;
                        return;
                    }
                    Invalidated?.Invoke(this, Bounds);
                }
                else
                {
                    DrawPlaceholder(canvas, "Loading...");
                    return;
                }
            }

            if (_bitmap == null)
            {
                DrawPlaceholder(canvas, string.IsNullOrEmpty(_filePath) ? "Empty" : "Loading...");
                return;
            }

            if (float.IsNaN(_bounds.Left) || float.IsNaN(_bounds.Top) ||
                float.IsNaN(_bounds.Right) || float.IsNaN(_bounds.Bottom) ||
                _bounds.Width <= 0 || _bounds.Height <= 0)
            {
                return;
            }

            if (_disposed) return;

            try
            {
                var paint = RenderResourcePool.Shared.RentPaint();
                paint.Color = new SKColor(0xFF, 0x00, 0x00, 0xFF);
                canvas.DrawRect(_bounds, paint);
                canvas.DrawBitmap(_bitmap, _bounds, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None), paint);
                RenderResourcePool.Shared.ReturnPaint(paint);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to draw image bitmap: {FilePath}", _filePath);
            }
        }

        private void DrawPlaceholder(SKCanvas canvas, string label)
        {
            if (float.IsNaN(_bounds.Left) || float.IsNaN(_bounds.Top) ||
                float.IsNaN(_bounds.Right) || float.IsNaN(_bounds.Bottom) ||
                _bounds.Width <= 0 || _bounds.Height <= 0)
            {
                return;
            }

            var bg = RenderResourcePool.Shared.RentPaint();
            bg.Color = new SKColor(50, 50, 50, 200);
            bg.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, bg);
            RenderResourcePool.Shared.ReturnPaint(bg);

            var font = RenderResourcePool.Shared.RentFont(13f);
            font.Typeface = RenderResourcePool.BoldTypeface;
            float tw = font.MeasureText(label);
            float lx = _bounds.MidX - tw / 2;
            float ly = _bounds.MidY + 5;
            var textPaint = RenderResourcePool.Shared.RentPaint();
            textPaint.Color = new SKColor(180, 180, 180, 255);
            canvas.DrawText(label, lx, ly, SKTextAlign.Left, font, textPaint);
            RenderResourcePool.Shared.ReturnPaint(textPaint);
            RenderResourcePool.Shared.ReturnFont(font);
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
            if (_disposed) return;
            _disposed = true;

            if (Interlocked.Exchange(ref _pdcDecremented, 1) == 0)
                Interlocked.Decrement(ref PendingDecodeCount);

            if (_cache == null && _bitmap != null)
            {
                _bitmap.Dispose();
            }
            _bitmap = null;
            _decodeTask = null;
        }
    }
}
