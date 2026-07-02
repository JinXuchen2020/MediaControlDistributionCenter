using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MediaControlDistributionCenter.Rendering
{
    internal sealed class SharedBitmap
    {
        private int _refCount = 1;
        public SKBitmap Bitmap { get; }
        public SharedBitmap(SKBitmap bitmap) => Bitmap = bitmap;
        public SKBitmap AddRef() { Interlocked.Increment(ref _refCount); return Bitmap; }
        public void Release()
        {
            if (Interlocked.Decrement(ref _refCount) == 0)
                Bitmap.Dispose();
        }
    }

    public class BitmapCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, SharedBitmap> _cache = new();
        private readonly int _maxEntries;
        private readonly LinkedList<string> _accessOrder = [];
        private readonly Dictionary<string, DateTime> _timestamps = new();
        private readonly TimeSpan _ttl;

        public BitmapCache(int maxEntries = 50, int ttlSeconds = 300)
        {
            _maxEntries = maxEntries;
            _ttl = TimeSpan.FromSeconds(ttlSeconds);
        }

        public SKBitmap? GetOrDecode(string filePath)
        {
            lock (_accessOrder)
            {
                if (_cache.TryGetValue(filePath, out var shared))
                {
                    if (_timestamps.TryGetValue(filePath, out var ts) && (DateTime.UtcNow - ts) > _ttl)
                    {
                        _cache.TryRemove(filePath, out _);
                        _accessOrder.Remove(filePath);
                        _timestamps.Remove(filePath);
                        shared.Release();
                        shared = null;
                    }
                    else
                    {
                        _accessOrder.Remove(filePath);
                        _accessOrder.AddLast(filePath);
                        _timestamps[filePath] = DateTime.UtcNow;
                        return shared.AddRef();
                    }
                }
            }

            if (!File.Exists(filePath)) return null;
            var decoded = SKBitmap.Decode(filePath);
            if (decoded == null) return null;

            var wrapped = new SharedBitmap(decoded);

            lock (_accessOrder)
            {
                if (_cache.Count >= _maxEntries)
                {
                    var oldest = _accessOrder.First;
                    if (oldest != null)
                    {
                        var key = oldest.Value;
                        _accessOrder.RemoveFirst();
                        if (_cache.TryRemove(key, out var old))
                            old.Release();
                    }
                }
                _cache[filePath] = wrapped;
                _accessOrder.AddLast(filePath);
                _timestamps[filePath] = DateTime.UtcNow;
            }

            return wrapped.AddRef();
        }

        public void Release(string filePath)
        {
            if (_cache.TryGetValue(filePath, out var shared))
            {
                shared.Release();
            }
        }

        public void Dispose()
        {
            foreach (var kvp in _cache)
                kvp.Value.Release();
            _cache.Clear();
            _accessOrder.Clear();
            _timestamps.Clear();
        }
    }
}
