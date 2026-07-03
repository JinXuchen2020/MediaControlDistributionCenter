using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;

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
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public BaseComponentViewModel? ViewModel => null;
        public IReadOnlyList<IRenderable>? Children => null;

        public event Action<IRenderable, SKRect>? Invalidated;

        public void SetTarget(IRenderable? target)
        {
            _target = target;
            IsVisible = target != null;
        }

        public void Draw(SKCanvas canvas)
        {
            if (_target == null || !IsVisible) return;

            var b = _target.Bounds;

            var fillPaint = RenderResourcePool.Shared.RentPaint();
            fillPaint.Color = new SKColor(0xD8, 0xE8, 0xFF);
            fillPaint.Style = SKPaintStyle.Fill;

            var strokePaint = RenderResourcePool.Shared.RentPaint();
            strokePaint.Color = new SKColor(0x40, 0x48, 0x70);
            strokePaint.Style = SKPaintStyle.Stroke;
            strokePaint.StrokeWidth = 1;

            var borderPaint = RenderResourcePool.Shared.RentPaint();
            borderPaint.Color = new SKColor(0x30, 0x47, 0x9C);
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.StrokeWidth = 2;
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

            RenderResourcePool.Shared.ReturnPaint(fillPaint);
            RenderResourcePool.Shared.ReturnPaint(strokePaint);
            RenderResourcePool.Shared.ReturnPaint(borderPaint);
        }

        public bool HitTest(SKPoint point)
        {
            if (!IsVisible) return false;
            foreach (var handle in _handles)
            {
                if (handle.Contains(point)) return true;
            }
            return false;
        }

        public int HitTestHandleIndex(SKPoint point)
        {
            if (!IsVisible) return -1;
            for (int i = 0; i < _handles.Count; i++)
            {
                if (_handles[i].Contains(point))
                    return i;
            }
            return -1;
        }

        public void Invalidate()
        {
            Invalidated?.Invoke(this, Bounds);
        }

        public void Dispose()
        {
        }
    }
}
