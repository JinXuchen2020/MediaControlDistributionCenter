using SkiaSharp;
using System.Collections.Concurrent;

namespace MediaControlDistributionCenter.Rendering
{
    public static class TextLayoutCache
    {
        private static readonly ConcurrentDictionary<string, float> _widthCache = new();

        public static float MeasureText(string text, SKFont font)
        {
            string key = $"{text}|{font.Size}|{font.Typeface?.FamilyName}|{font.SkewX}";
            return _widthCache.GetOrAdd(key, _ => font.MeasureText(text));
        }

        public static void Clear()
        {
            _widthCache.Clear();
        }
    }
}
