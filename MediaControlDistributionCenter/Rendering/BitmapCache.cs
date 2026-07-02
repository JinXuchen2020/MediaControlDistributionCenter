using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace MediaControlDistributionCenter.Rendering
{
    public class BitmapCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, SKBitmap> _cache = new();
        private readonly int _maxEntries;
        private readonly List<string> _accessOrder = [];
        private readonly Dictionary<string, DateTime> _timestamps = new();
        private readonly TimeSpan _ttl;

        public BitmapCache(int maxEntries = 50, int ttlSeconds = 300)
        {
            _maxEntries = maxEntries;
            _ttl = TimeSpan.FromSeconds(ttlSeconds);
        }

        public SKBitmap? GetOrDecode(string filePath)
        {
            if (_cache.TryGetValue(filePath, out var bitmap))
            {
                lock (_accessOrder)
                {
                    if (_timestamps.TryGetValue(filePath, out var ts) && (DateTime.UtcNow - ts) > _ttl)
                    {
                        _cache.TryRemove(filePath, out _);
                        _accessOrder.Remove(filePath);
                        _timestamps.Remove(filePath);
                        bitmap.Dispose();
                        bitmap = null;
                    }
                    else
                    {
                        return bitmap;
                    }
                }
            }

            if (!File.Exists(filePath)) return null;
            var decoded = SKBitmap.Decode(filePath);
            if (decoded == null) return null;

            lock (_accessOrder)
            {
                if (_cache.Count >= _maxEntries)
                {
                    var oldest = _accessOrder[0];
                    _accessOrder.RemoveAt(0);
                    if (_cache.TryRemove(oldest, out var old))
                        old.Dispose();
                }
                _cache[filePath] = decoded;
                _accessOrder.Add(filePath);
                _timestamps[filePath] = DateTime.UtcNow;
            }
            return decoded;
        }

        public void Dispose()
        {
            foreach (var kvp in _cache)
                kvp.Value.Dispose();
            _cache.Clear();
            _accessOrder.Clear();
            _timestamps.Clear();
        }
    }
}
