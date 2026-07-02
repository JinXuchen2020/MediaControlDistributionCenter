using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

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
        public RenderResourcePool ResourcePool { get; } = new();

        private DateTime _lastFrameTime = DateTime.UtcNow;
        private float _lastDeltaSeconds;

        public float LastDeltaSeconds => _lastDeltaSeconds;

        public SkiaCanvasController(IServiceProvider? services)
        {
            AnimationEngine = services?.GetRequiredService<AnimationEngine>() ?? new AnimationEngine();
            RenderEngine = services?.GetRequiredService<SkiaRenderEngine>() ?? new SkiaRenderEngine(AnimationEngine);
            Registry = services?.GetRequiredService<RenderableRegistry>() ?? new RenderableRegistry();
            _lastFrameTime = DateTime.UtcNow;
            RenderEngine.SurfacePool = SurfacePool;
        }

        public float UpdateDeltaTime()
        {
            var now = DateTime.UtcNow;
            _lastDeltaSeconds = (float)(now - _lastFrameTime).TotalSeconds;
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
            Registry.Register(new ImageComponentFactory(BitmapCache));
            Registry.Register(new ColorTextComponentFactory());
            Registry.Register(new TextComponentFactory());
            Registry.Register(new RssComponentFactory());
            Registry.Register(new WordComponentFactory());
            Registry.Register(new VideoComponentFactory());
            Registry.Register(new WebComponentFactory());
            Registry.Register(new StreamComponentFactory());
            Registry.Register(new HdmiComponentFactory());
        }

        public void Dispose()
        {
            RenderEngine.Clear();
            AnimationEngine.StopAll();
            BitmapCache.Dispose();
            ResourcePool.Dispose();
        }
    }
}
