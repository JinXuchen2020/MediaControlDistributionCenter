using SkiaSharp;
using System.Collections.Concurrent;

namespace MediaControlDistributionCenter.Rendering
{
    public class RenderResourcePool : IDisposable
    {
        public static readonly RenderResourcePool Shared = new();

        private readonly ConcurrentBag<SKPaint> _paints = new();
        private readonly ConcurrentBag<SKFont> _fonts = new();
        private readonly int _maxPerType;
        private int _paintCount;
        private int _fontCount;

        public RenderResourcePool(int maxPerType = 32)
        {
            _maxPerType = maxPerType;
        }

        public SKPaint RentPaint()
        {
            if (_paints.TryTake(out var paint))
            {
                Interlocked.Decrement(ref _paintCount);
                return paint;
            }
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
                font.Size = size;
                font.SkewX = 0;
                font.Typeface = null;
                return font;
            }
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

        public void Dispose()
        {
            while (_paints.TryTake(out var paint)) paint.Dispose();
            while (_fonts.TryTake(out var font)) font.Dispose();
            _paintCount = 0;
            _fontCount = 0;
            _boldTypeface?.Dispose();
        }

        private static readonly SKTypeface _boldTypeface = SKTypeface.FromFamilyName(
            null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        internal static SKTypeface BoldTypeface => _boldTypeface;
    }
}
