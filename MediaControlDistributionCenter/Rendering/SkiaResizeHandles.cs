using SkiaSharp;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaResizeHandles : IRenderable
    {
        private IRenderable? _target;
        private readonly List<SKRect> _handles = new();

        public string Type => "ResizeHandles";
        public int ZIndex { get; set; } = int.MaxValue;
        public SKRect Bounds
        {
            get
            {
                if (_target == null) return SKRect.Empty;
                var b = _target.Bounds;
                return new SKRect(b.Left - 12, b.Top - 12, b.Right + 12, b.Bottom + 12);
            }
        }
        public bool IsVisible { get; set; }

        public void SetTarget(IRenderable? target)
        {
            _target = target;
            IsVisible = target != null;
            UpdateHandles();
        }

        public void Draw(SKCanvas canvas)
        {
            if (_target == null || !IsVisible) return;

            var b = _target.Bounds;

            using var fillPaint = new SKPaint
            {
                Color = new SKColor(0xD8, 0xE8, 0xFF),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var strokePaint = new SKPaint
            {
                Color = new SKColor(0x40, 0x48, 0x70),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };

            // Selection border
            using var borderPaint = new SKPaint
            {
                Color = new SKColor(0x30, 0x47, 0x9C),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2
            };
            canvas.DrawRect(b, borderPaint);

            float s = 10f;
            float r = 8f;

            SKPoint[] positions =
            {
                new(b.Left, b.Top), new(b.Right, b.Top),
                new(b.Left, b.Bottom), new(b.Right, b.Bottom),
                new(b.Left, b.MidY), new(b.Right, b.MidY),
                new(b.MidX, b.Top), new(b.MidX, b.Bottom),
            };

            _handles.Clear();
            foreach (var pos in positions)
            {
                var rect = new SKRect(pos.X - s, pos.Y - s, pos.X + s, pos.Y + s);
                _handles.Add(rect);

                var roundRect = new SKRoundRect(rect, r);
                canvas.DrawRoundRect(roundRect, fillPaint);
                canvas.DrawRoundRect(roundRect, strokePaint);
            }
        }

        public bool HitTest(SKPoint point)
        {
            if (!IsVisible) return false;
            foreach (var handle in _handles)
            {
                if (handle.Contains(point)) return true;
            }
            return _target?.Bounds.Contains(point) ?? false;
        }

        public void Invalidate() { }

        public void Dispose()
        {
        }

        private void UpdateHandles() { }
    }
}
