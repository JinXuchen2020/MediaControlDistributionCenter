using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class SurfacePool : IDisposable
    {
        private SKSurface? _surface;
        private SKImageInfo _lastInfo;
        private GRContext? _grContext;

        public SurfacePool(GRContext? grContext = null)
        {
            _grContext = grContext;
        }

        public SKSurface GetOrCreate(SKImageInfo info)
        {
            if (_surface != null && _lastInfo == info)
            {
                _surface.Canvas.Clear(SKColors.Transparent);
                return _surface;
            }
            _surface?.Dispose();
            _lastInfo = info;

            if (_grContext != null)
            {
                _surface = SKSurface.Create(_grContext, false, info);
            }

            _surface ??= SKSurface.Create(info);
            return _surface;
        }

        public void UpdateContext(GRContext? grContext)
        {
            _grContext = grContext;
        }

        public void Dispose()
        {
            _surface?.Dispose();
            _surface = null;
            _grContext = null; // GRContext owned by SkiaRenderEngine, do not dispose
        }
    }
}
