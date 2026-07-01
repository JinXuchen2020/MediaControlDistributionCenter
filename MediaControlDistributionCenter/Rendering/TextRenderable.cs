using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class TextRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly TextComponentViewModel _vm;
        private float _scrollOffset;

        public string Type => "Text";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;

        public TextRenderable(TextComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();
        }

        public void Draw(SKCanvas canvas)
        {
            string text = _vm.Source ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return;

            float fontSize = (float)(_vm.TextSize * _vm.Ratio);

            using var bgPaint = new SKPaint
            {
                Color = new SKColor(_vm.Background.R, _vm.Background.G, _vm.Background.B, _vm.Background.A),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            };
            canvas.DrawRect(_bounds, bgPaint);

            using var textPaint = new SKPaint
            {
                TextSize = fontSize,
                IsAntialias = true,
                SubpixelText = true,
                Color = new SKColor(_vm.Foreground.R, _vm.Foreground.G, _vm.Foreground.B, _vm.Foreground.A),
                TextAlign = SKTextAlign.Left,
            };

            float lineHeight = fontSize * 1.4f;
            float x = _bounds.Left + 4;
            float y = _bounds.Top + fontSize;

            if (_vm.PlayMode == "rollingLeft" || _vm.PlayMode == "rollingRight")
            {
                float textWidth = textPaint.MeasureText(text);
                float speed = _vm.RollingSpeed * 1.5f;
                float direction = _vm.PlayMode == "rollingRight" ? 1f : -1f;
                _scrollOffset += direction * speed;

                if (_vm.IsLoopEnabled)
                {
                    if (_scrollOffset > _bounds.Width) _scrollOffset -= _bounds.Width + textWidth;
                    if (_scrollOffset < -(textWidth + _bounds.Width)) _scrollOffset += _bounds.Width + textWidth;
                }

                float drawX = x + _scrollOffset;
                canvas.DrawText(text, drawX, y, textPaint);

                if (_vm.IsLoopEnabled && drawX + textWidth < _bounds.Right)
                    canvas.DrawText(text, drawX + _bounds.Width + textWidth, y, textPaint);
            }
            else
            {
                var lines = text.Split('\n');
                float lineSpacing = (float)_vm.LineSpacing * (float)_vm.Ratio;
                foreach (var line in lines)
                {
                    canvas.DrawText(line, x, y, textPaint);
                    y += lineHeight + lineSpacing;
                    if (y > _bounds.Bottom) break;
                }
            }
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
