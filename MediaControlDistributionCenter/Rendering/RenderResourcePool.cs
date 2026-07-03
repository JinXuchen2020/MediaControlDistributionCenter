using SkiaSharp;
using System.Collections.Concurrent;

namespace MediaControlDistributionCenter.Rendering
{
    public class RenderResourcePool : IDisposable
    {
        public static readonly RenderResourcePool Shared = new();

        private readonly ConcurrentBag<SKPaint> _paints = new();
        private readonly ConcurrentBag<SKFont> _fonts = new();
        private readonly ConcurrentBag<SKPath> _paths = new();
        private readonly int _maxPerType;
        private int _paintCount;
        private int _fontCount;
        private int _pathCount;
        public int PaintHits { get; private set; }
        public int PaintMisses { get; private set; }
        public int FontHits { get; private set; }
        public int FontMisses { get; private set; }

        public RenderResourcePool(int maxPerType = 32)
        {
            _maxPerType = maxPerType;
        }

        public SKPaint RentPaint()
        {
            if (_paints.TryTake(out var paint))
            {
                Interlocked.Decrement(ref _paintCount);
                Interlocked.Increment(ref PaintHits);
                return paint;
            }
            Interlocked.Increment(ref PaintMisses);
            return new SKPaint { IsAntialias = true };
        }

        public void ReturnPaint(SKPaint paint)
        {
            paint.Color = default;
            paint.Style = SKPaintStyle.Fill;
            paint.StrokeWidth = 0;
            paint.IsAntialias = true;
            paint.Shader = null;
            paint.ImageFilter = null;
            paint.ColorFilter = null;
            paint.MaskFilter = null;
            paint.PathEffect = null;
            int count = Interlocked.Increment(ref _paintCount);
            if (count <= _maxPerType)
            {
                _paints.Add(paint);
            }
            else
            {
                Interlocked.Decrement(ref _paintCount);
                paint.Dispose();
            }
        }

        public SKFont RentFont(float size)
        {
            if (_fonts.TryTake(out var font))
            {
                Interlocked.Decrement(ref _fontCount);
                Interlocked.Increment(ref FontHits);
                font.Size = size;
                font.SkewX = 0;
                font.Typeface = null;
                return font;
            }
            Interlocked.Increment(ref FontMisses);
            return new SKFont(null, size);
        }

        public void ReturnFont(SKFont font)
        {
            font.Typeface = null;
            font.SkewX = 0;
            int count = Interlocked.Increment(ref _fontCount);
            if (count <= _maxPerType)
            {
                _fonts.Add(font);
            }
            else
            {
                Interlocked.Decrement(ref _fontCount);
                font.Dispose();
            }
        }

        public SKPath RentPath()
        {
            if (_paths.TryTake(out var path))
            {
                Interlocked.Decrement(ref _pathCount);
                path.Reset();
                return path;
            }
            return new SKPath();
        }

        public void ReturnPath(SKPath path)
        {
            path.Reset();
            int count = Interlocked.Increment(ref _pathCount);
            if (count <= _maxPerType)
            {
                _paths.Add(path);
            }
            else
            {
                Interlocked.Decrement(ref _pathCount);
                path.Dispose();
            }
        }

        public void ResetStats()
        {
            PaintHits = 0;
            PaintMisses = 0;
            FontHits = 0;
            FontMisses = 0;
        }

        public void Dispose()
        {
            while (_paints.TryTake(out var paint)) paint.Dispose();
            while (_fonts.TryTake(out var font)) font.Dispose();
            while (_paths.TryTake(out var path)) path.Dispose();
            _paintCount = 0;
            _fontCount = 0;
            _pathCount = 0;
            _boldTypeface?.Dispose();
        }

        private static readonly SKTypeface _boldTypeface = SKTypeface.FromFamilyName(
            null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        private static readonly ConcurrentDictionary<string, SKTypeface> _typefaceCache = new();
        internal static SKTypeface BoldTypeface => GetCachedTypeface(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        internal static SKTypeface GetCachedTypeface(string familyName, SKFontStyleWeight weight = SKFontStyleWeight.Normal, SKFontStyleWidth width = SKFontStyleWidth.Normal, SKFontStyleSlant slant = SKFontStyleSlant.Upright)
        {
            string key = $"{familyName}|{(int)weight}|{(int)width}|{(int)slant}";
            return _typefaceCache.GetOrAdd(key, _ => SKTypeface.FromFamilyName(familyName, weight, width, slant));
        }
    }
}
