using SkiaSharp;
using System.Collections.Concurrent;
using System.Threading;

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
        private int _paintHits;
        private int _paintMisses;
        private int _fontHits;
        private int _fontMisses;
        public int PaintHits => _paintHits;
        public int PaintMisses => _paintMisses;
        public int FontHits => _fontHits;
        public int FontMisses => _fontMisses;

        public RenderResourcePool(int maxPerType = 32)
        {
            _maxPerType = maxPerType;
        }

        public SKPaint RentPaint()
        {
            if (_paints.TryTake(out var paint))
            {
                Interlocked.Decrement(ref _paintCount);
                Interlocked.Increment(ref _paintHits);
                return paint;
            }
            Interlocked.Increment(ref _paintMisses);
            return new SKPaint { IsAntialias = true };
        }

        public void ReturnPaint(SKPaint paint)
        {
            paint.Color = default;
            paint.Style = SKPaintStyle.Fill;
            paint.StrokeWidth = 0;
            paint.IsAntialias = true;
            paint.BlendMode = SKBlendMode.SrcOver;
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
                Interlocked.Increment(ref _fontHits);
                font.Size = size;
                font.SkewX = 0;
                font.Typeface = null;
                return font;
            }
            Interlocked.Increment(ref _fontMisses);
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
            Interlocked.Exchange(ref _paintHits, 0);
            Interlocked.Exchange(ref _paintMisses, 0);
            Interlocked.Exchange(ref _fontHits, 0);
            Interlocked.Exchange(ref _fontMisses, 0);
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
