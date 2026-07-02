using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaRenderEngine
    {
        private readonly List<IRenderable> _renderables = new();
        private readonly AnimationEngine _animationEngine;
        private float _canvasRatio = 1f;
        private static readonly Lazy<bool> _gpuAvailable = new(CheckGpuAvailability);

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
            int index = _renderables.BinarySearch(renderable, ZIndexComparer.Instance);
            if (index < 0) index = ~index;
            _renderables.Insert(index, renderable);
        }

        public void RemoveRenderable(IRenderable renderable)
        {
            _animationEngine.Stop(renderable);
            _renderables.Remove(renderable);
        }

        public void Clear()
        {
            _animationEngine.StopAll();
            foreach (var r in _renderables)
                r.Dispose();
            _renderables.Clear();
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

            foreach (var renderable in _renderables)
            {
                if (!renderable.IsVisible)
                    continue;

                Statistics.DrawCallsPerFrame++;
                int baseSaveCount = canvas.SaveCount;
                canvas.Save();
                try
                {
                    _animationEngine.ApplyAnimations(canvas, renderable);
                    renderable.Draw(canvas);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "RenderFrame: exception in {Type}", renderable.Type);
                }
                while (canvas.SaveCount > baseSaveCount)
                    canvas.Restore();
                Statistics.LayerSavesPerFrame += canvas.SaveCount - baseSaveCount;
            }

            Statistics.AnimatedElements = HasActiveAnimations ? _renderables.Count : 0;
            Statistics.FrameTimeMs = deltaSeconds * 1000;
        }

        public IRenderable? HitTest(SKPoint point)
        {
            for (int i = _renderables.Count - 1; i >= 0; i--)
            {
                if (_renderables[i].IsVisible && _renderables[i].HitTest(point))
                    return _renderables[i];
            }
            return null;
        }

        public void InvalidateAll()
        {
            foreach (var r in _renderables)
                r.Invalidate();
        }

        public SurfacePool? SurfacePool { get; set; }

        public byte[]? CaptureSnapshot(int width, int height)
        {
            if (_renderables.Count == 0) return null;

            var info = new SKImageInfo(width, height);
            var surface = SurfacePool?.GetOrCreate(info) ?? SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);

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

        private sealed class ZIndexComparer : IComparer<IRenderable>
        {
            public static readonly ZIndexComparer Instance = new();
            public int Compare(IRenderable? a, IRenderable? b)
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                return a.ZIndex.CompareTo(b.ZIndex);
            }
        }
    }
}
