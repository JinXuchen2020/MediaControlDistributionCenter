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
            paint.Shader = null;
            paint.ImageFilter = null;
            paint.ColorFilter = null;
            paint.MaskFilter = null;
            paint.PathEffect = null;
            if (_paintCount < _maxPerType)
            {
                _paints.Add(paint);
                Interlocked.Increment(ref _paintCount);
            }
            else
            {
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
            if (_fontCount < _maxPerType)
            {
                _fonts.Add(font);
                Interlocked.Increment(ref _fontCount);
            }
            else
            {
                font.Dispose();
            }
        }

        public void Dispose()
        {
            while (_paints.TryTake(out var paint)) paint.Dispose();
            while (_fonts.TryTake(out var font)) font.Dispose();
            _paintCount = 0;
            _fontCount = 0;
        }
    }
}
