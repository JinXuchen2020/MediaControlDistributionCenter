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

        public bool UseOverlay => true;

        public WebRenderable(BaseComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            using var bgPaint = new SKPaint
            {
                Color = new SKColor(30, 30, 30),
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(_bounds, bgPaint);

            float cx = _bounds.MidX;
            float cy = _bounds.MidY;
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.3f;
            if (size > 4f)
            {
                using var ringPaint = new SKPaint
                {
                    Color = new SKColor(100, 180, 255, 120),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = size * 0.12f
                };
                float r = size * 0.5f;
                canvas.DrawCircle(cx, cy, r, ringPaint);
                canvas.DrawCircle(cx, cy, r * 0.5f, ringPaint);

                using var linePaint = new SKPaint
                {
                    Color = new SKColor(100, 180, 255, 120),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = size * 0.08f
                };
                canvas.DrawLine(cx - r, cy, cx + r, cy, linePaint);
                canvas.DrawLine(cx, cy - r, cx, cy + r, linePaint);
            }

            using var borderPaint = new SKPaint
            {
                Color = new SKColor(60, 60, 60),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawRect(_bounds, borderPaint);
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
            _bounds = new SKRect(
                (float)(_vm.Left * _vm.Ratio),
                (float)(_vm.Top * _vm.Ratio),
                (float)((_vm.Left + _vm.Width) * _vm.Ratio),
                (float)((_vm.Top + _vm.Height) * _vm.Ratio));
        }
    }
}
