using SkiaSharp;
using System.Collections.Concurrent;
using System.Linq;

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

        public static void ClearForFontSize(float fontSize)
        {
            string suffix = $"|{fontSize}|";
            var keysToRemove = _widthCache.Keys.Where(k => k.Contains(suffix)).ToList();
            foreach (var key in keysToRemove)
                _widthCache.TryRemove(key, out _);
        }
    }
}
