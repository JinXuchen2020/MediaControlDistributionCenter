using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class SurfacePool : IDisposable
    {
        private SKSurface? _surface;
        private SKImageInfo _lastInfo;

        public SKSurface GetOrCreate(SKImageInfo info)
        {
            if (_surface != null && _lastInfo == info)
            {
                _surface.Canvas.Clear(SKColors.Transparent);
                return _surface;
            }
            _surface?.Dispose();
            _lastInfo = info;
            _surface = SKSurface.Create(info);
            return _surface;
        }

        public void Dispose()
        {
            _surface?.Dispose();
            _surface = null;
        }
    }
}
