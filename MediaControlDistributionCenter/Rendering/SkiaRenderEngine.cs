using SkiaSharp;
using System.Threading;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaRenderEngine : IDisposable
    {
        private readonly List<IRenderable> _renderables = new();
        private readonly AnimationEngine _animationEngine;
        private float _canvasRatio = 1f;
        private static readonly Lazy<bool> _gpuAvailable = new(CheckGpuAvailability);
        private bool _needsRedraw = true;
        private SKRect? _dirtyRect;
        private readonly ReaderWriterLockSlim _rwLock = new();
        private readonly SKPaint _globalAnimPaint = new() { Color = new SKColor(255, 255, 255, 255) };
        private readonly Dictionary<IRenderable, int> _renderableIndex = new();
        private int _lastPoolHits;
        private int _lastPoolMisses;

        public float CanvasRatio
        {
            get => _canvasRatio;
            set
            {
                _canvasRatio = value;
                InvalidateAll();
            }
        }

        public IReadOnlyList<IRenderable> Renderables => _renderables;

        public bool HasActiveAnimations => _animationEngine.HasActiveAnimations;
        public bool IsInteracting { get; set; }
        public bool NeedsRedraw => _needsRedraw || HasActiveAnimations || IsInteracting || ImageRenderable.PendingDecodeCount > 0;

        public RenderStatistics Statistics { get; } = new();

        public static bool IsGpuAvailable => _gpuAvailable.Value;

        public SkiaRenderEngine(AnimationEngine animationEngine)
        {
            _animationEngine = animationEngine;
        }

        private static bool CheckGpuAvailability()
        {
            try
            {
                using var grContext = GRContext.CreateGl();
                return grContext != null;
            }
            catch
            {
                return false;
            }
        }

        public void AddRenderable(IRenderable renderable)
        {
            _rwLock.EnterWriteLock();
            try
            {
                int index = _renderables.BinarySearch(renderable, ZIndexComparer.Instance);
                if (index < 0) index = ~index;
                _renderables.Insert(index, renderable);
                UpdateRenderableIndex();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            renderable.Invalidated += OnRenderableInvalidated;
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void ReplaceRenderables(IEnumerable<IRenderable> newRenderables)
        {
            var newSet = new List<IRenderable>(newRenderables);
            newSet.Sort(ZIndexComparer.Instance);

            _rwLock.EnterWriteLock();
            try
            {
                foreach (var r in _renderables)
                    r.Invalidated -= OnRenderableInvalidated;

                for (int i = _renderables.Count - 1; i >= 0; i--)
                {
                    if (!newSet.Contains(_renderables[i]))
                    {
                        _animationEngine.Stop(_renderables[i]);
                        _renderables[i].Dispose();
                        _renderables.RemoveAt(i);
                    }
                }

                int insertAfter = -1;
                foreach (var item in newSet)
                {
                    bool exists = _renderableIndex.TryGetValue(item, out int existing);
                    if (exists)
                    {
                        insertAfter = existing;
                        continue;
                    }

                    int insertAt = insertAfter + 1;
                    if (insertAt < _renderables.Count)
                    {
                        int idx = _renderables.BinarySearch(insertAt, _renderables.Count - insertAt, item, ZIndexComparer.Instance);
                        if (idx < 0) idx = ~idx;
                        _renderables.Insert(idx, item);
                    }
                    else
                    {
                        _renderables.Add(item);
                    }
                }
                foreach (var item in newSet)
                    item.Invalidated += OnRenderableInvalidated;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            UpdateRenderableIndex();
            _needsRedraw = true;
        }

        public void RemoveRenderable(IRenderable renderable)
        {
            _animationEngine.Stop(renderable);
            _rwLock.EnterWriteLock();
            try
            {
                _renderables.Remove(renderable);
                UpdateRenderableIndex();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            renderable.Invalidated -= OnRenderableInvalidated;
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void Clear()
        {
            _animationEngine.StopAll();
            _rwLock.EnterWriteLock();
            try
            {
                foreach (var r in _renderables)
                {
                    r.Invalidated -= OnRenderableInvalidated;
                    r.Dispose();
                }
                _renderables.Clear();
                _renderableIndex.Clear();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void DisposeRenderable(IRenderable renderable)
        {
            RemoveRenderable(renderable);
            renderable.Dispose();
        }

        public void RenderFrame(SKCanvas canvas, float deltaSeconds)
        {
            Statistics.ResetFrame();
            _animationEngine.Update(deltaSeconds);
            _needsRedraw = false;
            var dirty = _dirtyRect;
            _dirtyRect = null;

            bool hasAnims = HasActiveAnimations;

            _rwLock.EnterReadLock();
            try
            {
                if (hasAnims)
                {
                    int globalLayer = canvas.Save();
                    Statistics.LayerSavesPerFrame++;
                    foreach (var renderable in _renderables)
                    {
                        if (!renderable.IsVisible)
                            continue;
                        if (dirty.HasValue && !renderable.Bounds.IntersectsWith(dirty.Value))
                            continue;
                        Statistics.DrawCallsPerFrame++;
                        int baseCount = canvas.SaveCount;
                        canvas.Save();
                        try
                        {
                            if (renderable.ScaleX != 1f || renderable.ScaleY != 1f)
                            {
                                var b = renderable.Bounds;
                                float cx = b.MidX;
                                float cy = b.MidY;
                                canvas.Scale(renderable.ScaleX, renderable.ScaleY, cx, cy);
                            }
                            _animationEngine.ApplyAnimations(canvas, renderable);
                            renderable.Draw(canvas);
                            DrawChildren(canvas, renderable);
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error(ex, "RenderFrame: exception in {Type}", renderable.Type);
                        }
                        while (canvas.SaveCount > baseCount)
                            canvas.Restore();
                    }
                    while (canvas.SaveCount > globalLayer)
                        canvas.Restore();
                }
                else
                {
                    foreach (var renderable in _renderables)
                    {
                        if (!renderable.IsVisible)
                            continue;
                        if (dirty.HasValue && !renderable.Bounds.IntersectsWith(dirty.Value))
                            continue;
                        Statistics.DrawCallsPerFrame++;
                        try
                        {
                            if (renderable.ScaleX != 1f || renderable.ScaleY != 1f)
                            {
                                var b = renderable.Bounds;
                                float cx = b.MidX;
                                float cy = b.MidY;
                                canvas.Scale(renderable.ScaleX, renderable.ScaleY, cx, cy);
                            }
                            renderable.Draw(canvas);
                            DrawChildren(canvas, renderable);
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error(ex, "RenderFrame: exception in {Type}", renderable.Type);
                        }
                    }
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            Statistics.AnimatedElements = hasAnims ? _renderables.Count : 0;
            int currentHits = RenderResourcePool.Shared.PaintHits;
            int currentMisses = RenderResourcePool.Shared.PaintMisses;
            Statistics.PoolHitsPerFrame = currentHits - _lastPoolHits;
            Statistics.PoolMissesPerFrame = currentMisses - _lastPoolMisses;
            _lastPoolHits = currentHits;
            _lastPoolMisses = currentMisses;
            Statistics.FrameTimeMs = deltaSeconds * 1000;
        }

        public IRenderable? HitTest(SKPoint point)
        {
            _rwLock.EnterReadLock();
            try
            {
                for (int i = _renderables.Count - 1; i >= 0; i--)
                {
                    var r = _renderables[i];
                    if (!r.IsVisible) continue;
                    SKPoint transformed = point;
                    if (r.ScaleX != 1f || r.ScaleY != 1f)
                    {
                        var b = r.Bounds;
                        float cx = b.MidX, cy = b.MidY;
                        transformed = new SKPoint(
                            (point.X - cx) / r.ScaleX + cx,
                            (point.Y - cy) / r.ScaleY + cy);
                    }
                    if (r.HitTest(transformed))
                        return r;
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return null;
        }

        public void InvalidateAll()
        {
            _dirtyRect = null;
            _needsRedraw = true;
            _rwLock.EnterReadLock();
            try
            {
                foreach (var r in _renderables)
                    r.Invalidate();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        private void OnRenderableInvalidated(IRenderable renderable, SKRect rect)
        {
            _needsRedraw = true;
            InvalidateRect(rect);
        }

        public void InvalidateRect(SKRect rect)
        {
            if (_dirtyRect == null)
                _dirtyRect = rect;
            else
                _dirtyRect = SKRect.Union(_dirtyRect.Value, rect);
            _needsRedraw = true;
        }

        public SurfacePool? SurfacePool { get; set; }

        public byte[]? CaptureSnapshot(int width, int height)
        {
            if (_renderables.Count == 0) return null;

            var info = new SKImageInfo(width, height);
            var surface = SurfacePool?.GetOrCreate(info) ?? SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);

            RenderFrame(canvas, 0f);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            var result = data.ToArray();
            if (SurfacePool == null) surface.Dispose();
            return result;
        }

        private void UpdateRenderableIndex()
        {
            _renderableIndex.Clear();
            for (int i = 0; i < _renderables.Count; i++)
                _renderableIndex[_renderables[i]] = i;
        }

        public void PlayAnimation(IRenderable target, IAnimation animation)
        {
            _animationEngine.Play(target, animation);
        }

        public bool RenderFrameGpu(SKCanvas targetCanvas, int width, int height, float deltaSeconds, SKColor clearColor = default)
        {
            if (_grContext == null || _grContext.IsAbandoned)
            {
                try
                {
                    _grContext?.Dispose();
                    _grContext = GRContext.CreateGl();
                    if (SurfacePool != null)
                        SurfacePool.UpdateContext(_grContext);
                }
                catch { _grContext = null; }
            }
            if (_grContext == null) { RenderFrame(targetCanvas, deltaSeconds); return false; }

            try
            {
                var info = new SKImageInfo(width, height);
                var surface = SurfacePool?.GetOrCreate(info);
                if (surface == null) { RenderFrame(targetCanvas, deltaSeconds); return false; }

                var canvas = surface.Canvas;
                canvas.Clear(clearColor);
                RenderFrame(canvas, deltaSeconds);
                targetCanvas.DrawSurface(surface, 0, 0);
                _grContext.Flush();
                return true;
            }
            catch
            {
                RenderFrame(targetCanvas, deltaSeconds);
                return false;
            }
        }

        private static void DrawChildren(SKCanvas canvas, IRenderable parent)
        {
            if (parent.Children == null) return;
            foreach (var child in parent.Children)
            {
                if (!child.IsVisible) continue;
                int baseCount = canvas.SaveCount;
                canvas.Save();
                try { child.Draw(canvas); }
                catch (Exception ex) { Serilog.Log.Error(ex, "RenderFrame: child exception"); }
                while (canvas.SaveCount > baseCount) canvas.Restore();
            }
        }

        private GRContext? _grContext;

        internal GRContext? SharedGrContext => _grContext;

        public static SKSurface? TryCreateGpuSurface(int width, int height)
        {
            if (!IsGpuAvailable) return null;
            try
            {
                var grContext = GRContext.CreateGl();
                if (grContext == null) return null;
                var info = new SKImageInfo(width, height);
                return SKSurface.Create(grContext, false, info);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _rwLock?.Dispose();
        }

        private sealed class ZIndexComparer : IComparer<IRenderable>
        {
            public static readonly ZIndexComparer Instance = new();
            public int Compare(IRenderable? a, IRenderable? b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                int result = a.ZIndex.CompareTo(b.ZIndex);
                return result != 0 ? result : a.GetHashCode().CompareTo(b.GetHashCode());
            }
        }
    }
}
