using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class VideoRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;

        public string Type => "Video";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public BaseComponentViewModel? ViewModel => _vm;

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
                var path = new SKPath();
                path.MoveTo(cx - size * 0.4f, cy - size * 0.5f);
                path.LineTo(cx - size * 0.4f, cy + size * 0.5f);
                path.LineTo(cx + size * 0.5f, cy);
                path.Close();
                canvas.DrawPath(path, iconPaint);
                path.Dispose();
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
