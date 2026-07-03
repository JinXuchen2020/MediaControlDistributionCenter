using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        private bool TryGetCached(string filePath, out SKBitmap? bitmap)
        {
            bitmap = null;
            lock (_lock)
            {
                if (_cache.TryGetValue(filePath, out var entry))
                {
                    if ((DateTime.UtcNow - entry.Timestamp) > _ttl)
                    {
                        entry.Bitmap.Dispose();
                        _cache.Remove(filePath);
                        _accessOrder.Remove(filePath);
                        return false;
                    }
                    _accessOrder.Remove(filePath);
                    _accessOrder.Add(filePath);
                    bitmap = entry.Bitmap;
                    return true;
                }
            }
            return false;
        }

        private SKBitmap AddToCache(string filePath, SKBitmap bitmap)
        {
            lock (_lock)
            {
                if (_cache.Count >= _maxEntries)
                {
                    var oldest = _accessOrder[0];
                    _accessOrder.RemoveAt(0);
                    if (_cache.Remove(oldest, out var old))
                        old.Bitmap.Dispose();
                }
                _cache[filePath] = new CacheEntry { Bitmap = bitmap, Timestamp = DateTime.UtcNow };
                _accessOrder.Add(filePath);
            }
            return bitmap;
        }

        public SKBitmap? GetOrDecode(string filePath)
        {
            if (TryGetCached(filePath, out var cached))
                return cached;

            if (!File.Exists(filePath)) return null;
            using var stream = File.OpenRead(filePath);
            var decoded = SKBitmap.Decode(stream);
            if (decoded == null) return null;

            return AddToCache(filePath, decoded);
        }

        public Task<SKBitmap?> GetOrDecodeAsync(string filePath)
        {
            if (TryGetCached(filePath, out var cached))
                return Task.FromResult<SKBitmap?>(cached);

            return Task.Run(() =>
            {
                if (!File.Exists(filePath)) return null;
                using var stream = File.OpenRead(filePath);
                var decoded = SKBitmap.Decode(stream);
                if (decoded == null) return null;
                return AddToCache(filePath, decoded);
            });
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

        public void PreWarm(IEnumerable<string> filePaths)
        {
            foreach (var path in filePaths)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try { GetOrDecode(path); }
                    catch { }
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
