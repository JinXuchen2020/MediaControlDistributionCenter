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
        public bool NeedsRedraw => _needsRedraw || HasActiveAnimations || IsInteracting;

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
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
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
                    int existing = _renderables.IndexOf(item);
                    if (existing >= 0)
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
                    insertAfter = _renderables.IndexOf(item);
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            _needsRedraw = true;
        }

        public void RemoveRenderable(IRenderable renderable)
        {
            _animationEngine.Stop(renderable);
            _rwLock.EnterWriteLock();
            try
            {
                _renderables.Remove(renderable);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
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
                    r.Dispose();
                _renderables.Clear();
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
            Statistics.FrameTimeMs = deltaSeconds * 1000;
        }

        public IRenderable? HitTest(SKPoint point)
        {
            _rwLock.EnterReadLock();
            try
            {
                for (int i = _renderables.Count - 1; i >= 0; i--)
                {
                    if (_renderables[i].IsVisible && _renderables[i].HitTest(point))
                        return _renderables[i];
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

            _rwLock.EnterReadLock();
            try
            {
                foreach (var renderable in _renderables)
                {
                    if (!renderable.IsVisible) continue;
                    int baseSaveCount = canvas.SaveCount;
                    canvas.Save();
                    try
                    {
                        _animationEngine.ApplyAnimations(canvas, renderable);
                        renderable.Draw(canvas);
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error(ex, "CaptureSnapshot: exception in {Type}", renderable.Type);
                    }
                    while (canvas.SaveCount > baseSaveCount)
                        canvas.Restore();
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            var result = data.ToArray();
            if (SurfacePool == null) surface.Dispose();
            return result;
        }

        public void PlayAnimation(IRenderable target, IAnimation animation)
        {
            _animationEngine.Play(target, animation);
        }

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
