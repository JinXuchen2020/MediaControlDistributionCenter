using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.IO;

namespace MediaControlDistributionCenter.Rendering
{
    public class TextRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly TextComponentViewModel _vm;
        private float _scrollOffset;
        private List<FormattedRun>? _runs;
        private volatile bool _runsLoaded;
        private List<float>? _measuredWidths;
        private float _totalWidth;
        private DateTime _lastScrollTime = DateTime.UtcNow;

        public string Type => "Text";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public BaseComponentViewModel? ViewModel => _vm;

        public event Action<IRenderable>? Invalidated;

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

            ComputeTotalWidth();
        }

        private void ComputeTotalWidth()
        {
            _totalWidth = 0;
            if (_runs == null) return;
            float scale = (float)_vm.Ratio;
            float fontSize = (float)_vm.TextSize * scale;
            _measuredWidths = new List<float>(_runs.Count);
            foreach (var run in _runs)
            {
                if (run.Text == "\n") { _measuredWidths.Add(0); continue; }
                using var font = CreateFont(run, fontSize, scale);
                var w = font.Value.MeasureText(run.Text);
                _measuredWidths.Add(w);
                _totalWidth += w;
            }
        }

        public void Draw(SKCanvas canvas)
        {
            EnsureRunsLoaded();
            if (_runs == null || _runs.Count == 0) return;

            float scale = (float)_vm.Ratio;

            var bgPaint = RenderResourcePool.Shared.RentPaint();
            bgPaint.Color = new SKColor(_vm.Background.R, _vm.Background.G, _vm.Background.B, _vm.Background.A);
            bgPaint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, bgPaint);
            RenderResourcePool.Shared.ReturnPaint(bgPaint);

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
            float speed = _vm.RollingSpeed * 1.5f;
            float direction = _vm.PlayMode == "rollingRight" ? 1f : -1f;
            var now = DateTime.UtcNow;
            float elapsed = (float)(now - _lastScrollTime).TotalSeconds;
            _lastScrollTime = now;
            if (elapsed > 0.1f) elapsed = 0.016f;
            _scrollOffset += direction * speed * elapsed;

            if (_vm.IsLoopEnabled)
            {
                if (_scrollOffset > _bounds.Width)
                    _scrollOffset -= _bounds.Width + _totalWidth;
                if (_scrollOffset < -(_totalWidth + _bounds.Width))
                    _scrollOffset += _bounds.Width + _totalWidth;
            }
            else
            {
                _scrollOffset = Math.Clamp(_scrollOffset, -(_totalWidth + 10), _bounds.Width);
            }

            float drawX = _bounds.Left + 4 + _scrollOffset;
            float y = _bounds.Top + (_bounds.Height + fontSize * 1.4f) / 2;

            for (int i = 0; i < _runs.Count; i++)
            {
                if (_runs[i].Text == "\n") continue;
                using var paint = CreatePaint(_runs[i], fontSize, scale);
                using var font = CreateFont(_runs[i], fontSize, scale);
                canvas.DrawText(_runs[i].Text, drawX, y, font.Value, paint.Value);
                drawX += _measuredWidths[i];
            }

            if (_vm.IsLoopEnabled)
            {
                float secondX = _bounds.Left + 4 + _scrollOffset + _totalWidth + _bounds.Width;
                for (int i = 0; i < _runs.Count; i++)
                {
                    if (_runs[i].Text == "\n") continue;
                    using var paint = CreatePaint(_runs[i], fontSize, scale);
                    using var font = CreateFont(_runs[i], fontSize, scale);
                    canvas.DrawText(_runs[i].Text, secondX, y, font.Value, paint.Value);
                    secondX += _measuredWidths[i];
                }
            }

            Invalidated?.Invoke(this);
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
                using var font = CreateFont(run, runFontSize, scale);
                float textWidth = font.Value.MeasureText(run.Text);

                if (x + textWidth > maxX && x > startX)
                {
                    x = startX;
                    y += lineHeight + lineSpacing;
                    if (y > maxY) break;
                }

                canvas.DrawText(run.Text, x, y, font.Value, paint.Value);
                x += textWidth;
            }
        }

        private struct PooledPaint : IDisposable
        {
            public SKPaint Value { get; }
            public PooledPaint(SKPaint paint) => Value = paint;
            public void Dispose() => RenderResourcePool.Shared.ReturnPaint(Value);
        }

        private struct PooledFont : IDisposable
        {
            public SKFont Value { get; }
            public PooledFont(SKFont font) => Value = font;
            public void Dispose() => RenderResourcePool.Shared.ReturnFont(Value);
        }

        private PooledPaint CreatePaint(FormattedRun run, float fontSize, float scale)
        {
            var paint = RenderResourcePool.Shared.RentPaint();
            paint.Color = run.Foreground;
            return new PooledPaint(paint);
        }

        private PooledFont CreateFont(FormattedRun run, float fontSize, float scale)
        {
            var font = RenderResourcePool.Shared.RentFont(fontSize);
            if (run.IsBold && run.IsItalic)
            {
                font.Typeface = RenderResourcePool.GetCachedTypeface(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
            }
            else if (run.IsBold)
            {
                font.Typeface = RenderResourcePool.BoldTypeface;
            }
            else if (run.IsItalic)
            {
                font.SkewX = -0.25f;
            }
            return new PooledFont(font);
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            _runsLoaded = false;
            _runs = null;
            _measuredWidths = null;
            UpdateBounds();
            Invalidated?.Invoke(this);
        }

        public void Dispose()
        {
            _runs = null;
            _measuredWidths = null;
        }

        public void UpdateBounds()
        {
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }
    }
}
