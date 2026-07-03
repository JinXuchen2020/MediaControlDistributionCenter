using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class StreamRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;
        private SKPath? _cachedPath1;
        private SKPath? _cachedPath2;
        private float _cachedSize;

        public string Type => "Stream";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public BaseComponentViewModel? ViewModel => _vm;

        public StreamRenderable(BaseComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();
        }

        private void EnsureCachedPaths(float cx, float cy, float size)
        {
            if (_cachedPath1 != null && Math.Abs(_cachedSize - size) <= 0.5f)
                return;

            _cachedPath1?.Dispose();
            _cachedPath2?.Dispose();

            float r = size * 0.35f;
            _cachedPath1 = RenderResourcePool.Shared.RentPath();
            _cachedPath1.AddArc(new SKRect(cx - r, cy - r, cx + r, cy + r), -60, 120);

            float r2 = size * 0.55f;
            _cachedPath2 = RenderResourcePool.Shared.RentPath();
            _cachedPath2.AddArc(new SKRect(cx - r2, cy - r2, cx + r2, cy + r2), -60, 120);

            _cachedSize = size;
        }

        public void Draw(SKCanvas canvas)
        {
            var paint = RenderResourcePool.Shared.RentPaint();
            paint.Color = SKColors.Black;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, paint);
            RenderResourcePool.Shared.ReturnPaint(paint);

            float cx = _bounds.MidX;
            float cy = _bounds.MidY;
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.3f;
            if (size > 4f)
            {
                var dotPaint = RenderResourcePool.Shared.RentPaint();
                dotPaint.Color = new SKColor(80, 220, 120, 140);
                dotPaint.Style = SKPaintStyle.Fill;
                canvas.DrawCircle(cx, cy, size * 0.15f, dotPaint);

                var arcPaint = RenderResourcePool.Shared.RentPaint();
                arcPaint.Color = new SKColor(80, 220, 120, 100);
                arcPaint.Style = SKPaintStyle.Stroke;
                arcPaint.StrokeWidth = size * 0.08f;

                EnsureCachedPaths(cx, cy, size);
                canvas.DrawPath(_cachedPath1, arcPaint);
                canvas.DrawPath(_cachedPath2, arcPaint);

                RenderResourcePool.Shared.ReturnPaint(dotPaint);
                RenderResourcePool.Shared.ReturnPaint(arcPaint);
            }

            var borderPaint = RenderResourcePool.Shared.RentPaint();
            borderPaint.Color = new SKColor(60, 60, 60);
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.StrokeWidth = 1;
            canvas.DrawRect(_bounds, borderPaint);
            RenderResourcePool.Shared.ReturnPaint(borderPaint);
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            _cachedPath1?.Dispose();
            _cachedPath1 = null;
            _cachedPath2?.Dispose();
            _cachedPath2 = null;
            UpdateBounds();
        }

        public void Dispose()
        {
            _cachedPath1?.Dispose();
            _cachedPath2?.Dispose();
        }

        public void UpdateBounds()
        {
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }
    }
}
