using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace MediaControlDistributionCenter.Rendering
{
    public class BitmapCache : IDisposable
    {
        private readonly Dictionary<string, CacheEntry> _cache = new();
        private readonly List<string> _accessOrder = new();
        private readonly int _maxEntries;
        private readonly TimeSpan _ttl;
        private readonly object _lock = new();

        private struct CacheEntry
        {
            public SKBitmap Bitmap;
            public DateTime Timestamp;
        }

        public BitmapCache(int maxEntries = 50, int ttlSeconds = 300)
        {
            _maxEntries = maxEntries;
            _ttl = TimeSpan.FromSeconds(ttlSeconds);
        }

        public SKBitmap? GetOrDecode(string filePath)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(filePath, out var entry))
                {
                    if ((DateTime.UtcNow - entry.Timestamp) > _ttl)
                    {
                        entry.Bitmap.Dispose();
                        _cache.Remove(filePath);
                        _accessOrder.Remove(filePath);
                    }
                    else
                    {
                        _accessOrder.Remove(filePath);
                        _accessOrder.Add(filePath);
                        return entry.Bitmap;
                    }
                }
            }

            if (!File.Exists(filePath)) return null;
            var decoded = SKBitmap.Decode(filePath);
            if (decoded == null) return null;

            lock (_lock)
            {
                if (_cache.Count >= _maxEntries)
                {
                    var oldest = _accessOrder[0];
                    _accessOrder.RemoveAt(0);
                    if (_cache.Remove(oldest, out var old))
                        old.Bitmap.Dispose();
                }
                _cache[filePath] = new CacheEntry { Bitmap = decoded, Timestamp = DateTime.UtcNow };
                _accessOrder.Add(filePath);
            }
            return decoded;
        }

        public void Release(string filePath)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(filePath, out var entry))
                {
                    _cache.Remove(filePath);
                    _accessOrder.Remove(filePath);
                    entry.Bitmap.Dispose();
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var kvp in _cache)
                    kvp.Value.Bitmap.Dispose();
                _cache.Clear();
                _accessOrder.Clear();
            }
        }
    }
}
