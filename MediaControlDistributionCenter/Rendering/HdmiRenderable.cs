using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class HdmiRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;

        public string Type => "Hdmi";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;

        public bool UseOverlay => true;

        public HdmiRenderable(BaseComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(_bounds, paint);

            float cx = _bounds.MidX;
            float cy = _bounds.MidY;
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.25f;
            if (size > 4f)
            {
                using var labelPaint = new SKPaint
                {
                    Color = new SKColor(255, 200, 80, 160),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = size * 0.12f
                };

                float hw = size * 0.6f;
                float hh = size * 0.4f;
                var rect = new SKRect(cx - hw, cy - hh, cx + hw, cy + hh);
                canvas.DrawRect(rect, labelPaint);

                using var fillPaint = new SKPaint
                {
                    Color = new SKColor(255, 200, 80, 60),
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawRect(rect, fillPaint);

                using var textPaint = new SKPaint
                {
                    Color = new SKColor(255, 200, 80, 200),
                    IsAntialias = true,
                    TextSize = size * 0.35f,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("HDMI", cx, cy + size * 0.12f, textPaint);
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
