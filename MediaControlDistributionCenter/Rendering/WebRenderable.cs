using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class WebRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;

        public string Type => "Web";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public BaseComponentViewModel? ViewModel => _vm;

        public WebRenderable(BaseComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            var bgPaint = RenderResourcePool.Shared.RentPaint();
            bgPaint.Color = new SKColor(30, 30, 30);
            bgPaint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, bgPaint);
            RenderResourcePool.Shared.ReturnPaint(bgPaint);

            float cx = _bounds.MidX;
            float cy = _bounds.MidY;
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.3f;
            if (size > 4f)
            {
                var ringPaint = RenderResourcePool.Shared.RentPaint();
                ringPaint.Color = new SKColor(100, 180, 255, 120);
                ringPaint.Style = SKPaintStyle.Stroke;
                ringPaint.StrokeWidth = size * 0.12f;
                float r = size * 0.5f;
                canvas.DrawCircle(cx, cy, r, ringPaint);
                canvas.DrawCircle(cx, cy, r * 0.5f, ringPaint);

                var linePaint = RenderResourcePool.Shared.RentPaint();
                linePaint.Color = new SKColor(100, 180, 255, 120);
                linePaint.Style = SKPaintStyle.Stroke;
                linePaint.StrokeWidth = size * 0.08f;
                canvas.DrawLine(cx - r, cy, cx + r, cy, linePaint);
                canvas.DrawLine(cx, cy - r, cx, cy + r, linePaint);

                RenderResourcePool.Shared.ReturnPaint(ringPaint);
                RenderResourcePool.Shared.ReturnPaint(linePaint);
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
            UpdateBounds();
        }

        public void Dispose()
        {
        }

        public void UpdateBounds()
        {
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }
    }
}
