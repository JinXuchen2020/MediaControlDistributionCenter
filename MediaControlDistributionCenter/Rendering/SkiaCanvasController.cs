using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System.Diagnostics;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaCanvasController : IDisposable
    {
        public SkiaRenderEngine RenderEngine { get; }
        public AnimationEngine AnimationEngine { get; }
        public RenderableRegistry Registry { get; }
        public FpsCounter FpsCounter { get; } = new();
        public BitmapCache BitmapCache { get; } = new();
        public SurfacePool SurfacePool { get; } = new();
        private long _lastFrameTime = Stopwatch.GetTimestamp();
        private float _lastDeltaSeconds;

        public float LastDeltaSeconds => _lastDeltaSeconds;

        public SkiaCanvasController(IServiceProvider? services)
        {
            AnimationEngine = services?.GetRequiredService<AnimationEngine>() ?? new AnimationEngine();
            RenderEngine = services?.GetRequiredService<SkiaRenderEngine>() ?? new SkiaRenderEngine(AnimationEngine);
            Registry = services?.GetRequiredService<RenderableRegistry>() ?? new RenderableRegistry(AnimationEngine);
            _lastFrameTime = Stopwatch.GetTimestamp();
            SurfacePool = new SurfacePool(RenderEngine.SharedGrContext);
            RenderEngine.SurfacePool = SurfacePool;
            try
            {
                var grContext = GRContext.CreateGl();
                if (grContext != null)
                {
                    RenderEngine.SetGrContext(grContext);
                    SurfacePool.UpdateContext(grContext);
                }
            }
            catch { }
        }

        public float UpdateDeltaTime()
        {
            var now = Stopwatch.GetTimestamp();
            _lastDeltaSeconds = (float)(now - _lastFrameTime) / Stopwatch.Frequency;
            _lastFrameTime = now;
            if (_lastDeltaSeconds > 0.1f) _lastDeltaSeconds = 0.016f;
            return _lastDeltaSeconds;
        }

        public void ToggleFps()
        {
            FpsCounter.IsVisible = !FpsCounter.IsVisible;
            if (FpsCounter.IsVisible) FpsCounter.Reset();
        }

        public void InitializeFactories()
        {
            Registry.RegisterDefaultFactories(BitmapCache);
        }

        public void Dispose()
        {
            RenderEngine.Clear();
            AnimationEngine.StopAll();
            BitmapCache.Dispose();
            SurfacePool.Dispose();
            FpsCounter.Dispose();
        }
    }
}
