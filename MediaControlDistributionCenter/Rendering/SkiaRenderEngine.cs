using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaRenderEngine : IDisposable
    {
        private volatile List<IRenderable> _renderables = new();
        private readonly AnimationEngine _animationEngine;
        private float _canvasRatio = 1f;
        private static readonly Lazy<bool> _gpuAvailable = new(CheckGpuAvailability);
        private bool _needsRedraw = true;
        private SKRect? _dirtyRect;
        // Lock-free: _renderables is volatile, writers swap via copy-modify-swap
        private readonly SKPaint _globalAnimPaint = new() { Color = new SKColor(255, 255, 255, 255) };
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
            var newList = new List<IRenderable>(_renderables);
            int index = newList.BinarySearch(renderable, ZIndexComparer.Instance);
            if (index < 0) index = ~index;
            newList.Insert(index, renderable);
            _renderables = newList;
            renderable.Invalidated += OnRenderableInvalidated;
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void AddRenderables(IEnumerable<IRenderable> renderables)
        {
            var newList = new List<IRenderable>(_renderables);
            foreach (var r in renderables)
                newList.Add(r);
            newList.Sort(ZIndexComparer.Instance);
            _renderables = newList;
            foreach (var r in renderables)
                r.Invalidated += OnRenderableInvalidated;
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void ReplaceRenderables(IEnumerable<IRenderable> newRenderables)
        {
            var newSet = new List<IRenderable>(newRenderables);
            newSet.Sort(ZIndexComparer.Instance);
            var newSetHash = new HashSet<IRenderable>(newSet);

            var oldList = _renderables;
            foreach (var r in oldList)
                r.Invalidated -= OnRenderableInvalidated;

            foreach (var r in oldList)
            {
                if (!newSetHash.Contains(r))
                {
                    _animationEngine.Stop(r);
                    r.Dispose();
                }
            }

            _renderables = newSet;

            foreach (var item in newSet)
                item.Invalidated += OnRenderableInvalidated;

            _needsRedraw = true;
        }

        public void RemoveRenderable(IRenderable renderable)
        {
            _animationEngine.Stop(renderable);
            var newList = new List<IRenderable>(_renderables);
            newList.Remove(renderable);
            _renderables = newList;
            renderable.Invalidated -= OnRenderableInvalidated;
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void Clear()
        {
            _animationEngine.StopAll();
            var oldList = _renderables;
            _renderables = new List<IRenderable>();
            foreach (var r in oldList)
            {
                r.Invalidated -= OnRenderableInvalidated;
                r.Dispose();
            }
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void DisposeRenderable(IRenderable renderable)
        {
            RemoveRenderable(renderable);
            renderable.Dispose();
        }

        public void UpdateZIndex(IRenderable renderable, int newZIndex)
        {
            renderable.ZIndex = newZIndex;
            var newList = new List<IRenderable>(_renderables);
            newList.Sort(ZIndexComparer.Instance);
            _renderables = newList;
            _needsRedraw = true;
            _dirtyRect = null;
        }

        public void RenderFrame(SKCanvas canvas, float deltaSeconds)
        {
            Statistics.ResetFrame();
            _animationEngine.DrainCompleted();
            _needsRedraw = false;
            var dirty = _dirtyRect;
            _dirtyRect = null;
            var snapshot = _renderables;

            bool hasAnims = HasActiveAnimations;

            if (hasAnims)
            {
                foreach (var renderable in snapshot)
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
            }
            else
            {
                foreach (var renderable in snapshot)
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

            Statistics.AnimatedElements = hasAnims ? snapshot.Count : 0;
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
            return HitTestRecursive(_renderables, point);
        }

        private static IRenderable? HitTestRecursive(IReadOnlyList<IRenderable> items, SKPoint point, int maxDepth = 16)
        {
            if (maxDepth <= 0) return null;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var r = items[i];
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

                if (r.Children != null && r.Children.Count > 0)
                {
                    var child = HitTestRecursive(r.Children, transformed, maxDepth - 1);
                    if (child != null) return child;
                }

                if (r.HitTest(transformed))
                    return r;
            }
            return null;
        }

        public void InvalidateAll()
        {
            _dirtyRect = null;
            _needsRedraw = true;
            foreach (var r in _renderables)
                r.Invalidate();
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
            var snapshot = _renderables;
            if (snapshot.Count == 0) return null;

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
            if (parent.Children == null || parent.Children.Count == 0) return;
            canvas.Save();
            canvas.ClipRect(parent.Bounds);
            foreach (var child in parent.Children)
            {
                if (!child.IsVisible) continue;
                int baseCount = canvas.SaveCount;
                canvas.Save();
                try { child.Draw(canvas); }
                catch (Exception ex) { Serilog.Log.Error(ex, "RenderFrame: child exception"); }
                while (canvas.SaveCount > baseCount) canvas.Restore();
            }
            canvas.Restore();
        }

        private GRContext? _grContext;

        internal GRContext? SharedGrContext => _grContext;

        internal void SetGrContext(GRContext? context)
        {
            _grContext?.Dispose();
            _grContext = context;
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
            _grContext?.Dispose();
            _grContext = null;
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
