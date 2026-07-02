using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;

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
        public BaseComponentViewModel? ViewModel => _vm;

        public StreamRenderable(BaseComponentViewModel vm)
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
                float r = size * 0.35f;
                var path = new SKPath();
                path.AddArc(new SKRect(cx - r, cy - r, cx + r, cy + r), -60, 120);
                canvas.DrawPath(path, arcPaint);

                float r2 = size * 0.55f;
                var path2 = new SKPath();
                path2.AddArc(new SKRect(cx - r2, cy - r2, cx + r2, cy + r2), -60, 120);
                canvas.DrawPath(path2, arcPaint);

                path.Dispose();
                path2.Dispose();
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
