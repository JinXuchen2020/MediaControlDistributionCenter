using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;

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
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public BaseComponentViewModel? ViewModel => _vm;
        public IReadOnlyList<IRenderable>? Children => null;

        public event Action<IRenderable, SKRect>? Invalidated;

        public HdmiRenderable(BaseComponentViewModel vm)
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
            float size = Math.Min(_bounds.Width, _bounds.Height) * 0.25f;
            if (size > 4f)
            {
                var labelPaint = RenderResourcePool.Shared.RentPaint();
                labelPaint.Color = new SKColor(255, 200, 80, 160);
                labelPaint.Style = SKPaintStyle.Stroke;
                labelPaint.StrokeWidth = size * 0.12f;

                float hw = size * 0.6f;
                float hh = size * 0.4f;
                var rect = new SKRect(cx - hw, cy - hh, cx + hw, cy + hh);
                canvas.DrawRect(rect, labelPaint);
                RenderResourcePool.Shared.ReturnPaint(labelPaint);

                var fillPaint = RenderResourcePool.Shared.RentPaint();
                fillPaint.Color = new SKColor(255, 200, 80, 60);
                fillPaint.Style = SKPaintStyle.Fill;
                canvas.DrawRect(rect, fillPaint);
                RenderResourcePool.Shared.ReturnPaint(fillPaint);

                var textPaint = RenderResourcePool.Shared.RentPaint();
                textPaint.Color = new SKColor(255, 200, 80, 200);
                var textFont = RenderResourcePool.Shared.RentFont(size * 0.35f);
                canvas.DrawText("HDMI", cx, cy + size * 0.12f, SKTextAlign.Center, textFont, textPaint);
                RenderResourcePool.Shared.ReturnFont(textFont);
                RenderResourcePool.Shared.ReturnPaint(textPaint);
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
            Invalidated?.Invoke(this, Bounds);
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
