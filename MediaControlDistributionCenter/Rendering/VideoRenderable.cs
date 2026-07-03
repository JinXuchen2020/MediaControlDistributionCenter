using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class VideoRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;
        private SKPath? _cachedPath;
        private float _cachedSize;

        public string Type => "Video";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public BaseComponentViewModel? ViewModel => _vm;
        public IReadOnlyList<IRenderable>? Children => null;
        public bool IsDecoding => false;

        public event Action<IRenderable, SKRect>? Invalidated;

        public VideoRenderable(BaseComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            var paint = RenderResourcePool.Shared.RentPaint();
            paint.Color = SKColors.Black;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, paint);
            RenderResourcePool.Shared.ReturnPaint(paint);

            var iconPaint = RenderResourcePool.Shared.RentPaint();
            iconPaint.Color = new SKColor(255, 255, 255, 80);
            iconPaint.Style = SKPaintStyle.Fill;

            float cx = _bounds.MidX;
            float cy = _bounds.MidY;
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.3f;
            if (size > 4f)
            {
                if (_cachedPath == null || Math.Abs(_cachedSize - size) > 0.5f)
                {
                    _cachedPath?.Dispose();
                    _cachedPath = RenderResourcePool.Shared.RentPath();
                    _cachedPath.MoveTo(cx - size * 0.4f, cy - size * 0.5f);
                    _cachedPath.LineTo(cx - size * 0.4f, cy + size * 0.5f);
                    _cachedPath.LineTo(cx + size * 0.5f, cy);
                    _cachedPath.Close();
                    _cachedSize = size;
                }
                canvas.DrawPath(_cachedPath, iconPaint);
            }

            var borderPaint = RenderResourcePool.Shared.RentPaint();
            borderPaint.Color = new SKColor(60, 60, 60);
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.StrokeWidth = 1;
            canvas.DrawRect(_bounds, borderPaint);
            RenderResourcePool.Shared.ReturnPaint(iconPaint);
            RenderResourcePool.Shared.ReturnPaint(borderPaint);
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            _cachedPath?.Dispose();
            _cachedPath = null;
            UpdateBounds();
            Invalidated?.Invoke(this, Bounds);
        }

        public void Dispose()
        {
            _cachedPath?.Dispose();
        }

        public void UpdateBounds()
        {
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }
    }
}
