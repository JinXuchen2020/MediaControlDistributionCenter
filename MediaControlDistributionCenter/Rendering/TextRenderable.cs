using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class TextRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly TextComponentViewModel _vm;
        private float _scrollOffset;
        private List<FormattedRun>? _runs;
        private bool _runsLoaded;

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

        private void EnsureRunsLoaded()
        {
            if (_runsLoaded) return;
            _runsLoaded = true;

            if (!string.IsNullOrEmpty(_vm.RtfFilePath) && File.Exists(_vm.RtfFilePath))
            {
                _runs = RtfXamlParser.Parse(_vm.RtfFilePath);
                if (_runs.Count == 0)
                    _runs = RtfXamlParser.CreateFromPlainText(_vm.Source ?? "", (float)_vm.TextSize);
            }
            else
            {
                _runs = RtfXamlParser.CreateFromPlainText(_vm.Source ?? "", (float)_vm.TextSize);
            }
        }

        public void Draw(SKCanvas canvas)
        {
            EnsureRunsLoaded();
            if (_runs == null || _runs.Count == 0) return;

            float scale = (float)_vm.Ratio;

            using var bgPaint = new SKPaint
            {
                Color = new SKColor(_vm.Background.R, _vm.Background.G, _vm.Background.B, _vm.Background.A),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            };
            canvas.DrawRect(_bounds, bgPaint);

            float padding = 4 * scale;
            float x = _bounds.Left + padding;
            float y = _bounds.Top + padding;
            float maxX = _bounds.Right - padding;
            float maxY = _bounds.Bottom - padding;

            if (_vm.PlayMode == "rollingLeft" || _vm.PlayMode == "rollingRight")
            {
                DrawScrolling(canvas, scale);
            }
            else
            {
                DrawWrapped(canvas, x, y, maxX, maxY, scale);
            }
        }

        private void DrawScrolling(SKCanvas canvas, float scale)
        {
            if (_runs == null || _runs.Count == 0) return;

            float fontSize = (float)_vm.TextSize * scale;
            float lineHeight = fontSize * 1.4f;
            float speed = _vm.RollingSpeed * 1.5f;
            float direction = _vm.PlayMode == "rollingRight" ? 1f : -1f;
            _scrollOffset += direction * speed;

            float totalWidth = 0;
            foreach (var run in _runs)
            {
                if (run.Text == "\n") continue;
                using var paint = CreatePaint(run, fontSize, scale);
                totalWidth += paint.MeasureText(run.Text);
            }

            if (_vm.IsLoopEnabled)
            {
                if (_scrollOffset > _bounds.Width)
                    _scrollOffset -= _bounds.Width + totalWidth;
                if (_scrollOffset < -(totalWidth + _bounds.Width))
                    _scrollOffset += _bounds.Width + totalWidth;
            }

            float drawX = _bounds.Left + 4 + _scrollOffset;
            float y = _bounds.Top + (_bounds.Height + fontSize * scale * 1.4f) / 2;

            foreach (var run in _runs)
            {
                if (run.Text == "\n") continue;
                using var paint = CreatePaint(run, fontSize, scale);
                canvas.DrawText(run.Text, drawX, y, paint);
                drawX += paint.MeasureText(run.Text);
            }

            if (_vm.IsLoopEnabled)
            {
                float secondX = _bounds.Left + 4 + _scrollOffset + totalWidth + _bounds.Width;
                foreach (var run in _runs)
                {
                    if (run.Text == "\n") continue;
                    using var paint = CreatePaint(run, fontSize, scale);
                    canvas.DrawText(run.Text, secondX, y, paint);
                    secondX += paint.MeasureText(run.Text);
                }
            }
        }

        private void DrawWrapped(SKCanvas canvas, float startX, float startY, float maxX, float maxY, float scale)
        {
            if (_runs == null) return;

            float baseFontSize = (float)_vm.TextSize * scale;
            float lineHeight = baseFontSize * 1.4f;
            float lineSpacing = (float)_vm.LineSpacing * scale;
            float x = startX;
            float y = startY + baseFontSize;

            foreach (var run in _runs)
            {
                if (run.Text == "\n")
                {
                    x = startX;
                    y += lineHeight + lineSpacing;
                    if (y > maxY) break;
                    continue;
                }

                float runFontSize = run.FontSize * scale;
                using var paint = CreatePaint(run, runFontSize, scale);
                float textWidth = paint.MeasureText(run.Text);

                if (x + textWidth > maxX && x > startX)
                {
                    x = startX;
                    y += lineHeight + lineSpacing;
                    if (y > maxY) break;
                }

                canvas.DrawText(run.Text, x, y, paint);
                x += textWidth;
            }
        }

        private SKPaint CreatePaint(FormattedRun run, float fontSize, float scale)
        {
            var paint = new SKPaint
            {
                TextSize = fontSize,
                IsAntialias = true,
                SubpixelText = true,
                Color = run.Foreground,
                TextAlign = SKTextAlign.Left,
            };

            if (run.IsBold)
                paint.FakeBoldText = true;

            if (run.IsItalic)
                paint.TextSkewX = -0.25f;

            return paint;
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            _runsLoaded = false;
            _runs = null;
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
