using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class StreamRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;

        public string Type => "Stream";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;

        public bool UseOverlay => true;

        public StreamRenderable(BaseComponentViewModel vm)
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
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.3f;
            if (size > 4f)
            {
                using var dotPaint = new SKPaint
                {
                    Color = new SKColor(80, 220, 120, 140),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawCircle(cx, cy, size * 0.15f, dotPaint);

                using var arcPaint = new SKPaint
                {
                    Color = new SKColor(80, 220, 120, 100),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = size * 0.08f
                };
                float r = size * 0.35f;
                using var path = new SKPath();
                path.AddArc(new SKRect(cx - r, cy - r, cx + r, cy + r), -60, 120);
                canvas.DrawPath(path, arcPaint);

                float r2 = size * 0.55f;
                using var path2 = new SKPath();
                path2.AddArc(new SKRect(cx - r2, cy - r2, cx + r2, cy + r2), -60, 120);
                canvas.DrawPath(path2, arcPaint);
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
