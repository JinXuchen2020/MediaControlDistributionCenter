using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class FpsCounter
    {
        private readonly float[] _frameTimes = new float[120];
        private int _index;
        private int _count;
        private float _elapsed;
        private float _accumulator;
        public float CurrentFps { get; private set; }
        public float MinFps { get; private set; } = float.MaxValue;
        public float MaxFps { get; private set; }
        public bool IsVisible { get; set; }

        private readonly SKPaint _bgPaint;
        private readonly SKPaint _fgPaint;
        private readonly SKFont _font;
        private string? _lastText;
        private float _lastTextWidth;

        public FpsCounter()
        {
            _font = new SKFont(SKTypeface.Default, 14);
            _bgPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 140),
                Style = SKPaintStyle.Fill,
            };
            _fgPaint = new SKPaint
            {
                IsAntialias = true,
            };
        }

        public void Update(float deltaSeconds)
        {
            if (_count < 120)
            {
                _frameTimes[_index] = deltaSeconds;
                _accumulator += deltaSeconds;
                _index = (_index + 1) % 120;
                _count++;
            }
            else
            {
                _accumulator -= _frameTimes[_index];
                _frameTimes[_index] = deltaSeconds;
                _accumulator += deltaSeconds;
                _index = (_index + 1) % 120;
            }

            _elapsed += deltaSeconds;

            if (_elapsed >= 0.5f && _count > 0)
            {
                float avg = _accumulator / _count;
                CurrentFps = avg > 0 ? 1f / avg : 0;
                if (CurrentFps < MinFps) MinFps = CurrentFps;
                if (CurrentFps > MaxFps) MaxFps = CurrentFps;
                _elapsed = 0;
            }
        }

        public void Draw(SKCanvas canvas, float canvasWidth)
        {
            if (!IsVisible) return;

            var bgRect = new SKRect(canvasWidth - 240, 2, canvasWidth - 4, 28);
            canvas.DrawRoundRect(new SKRoundRect(bgRect, 4), _bgPaint);

            var color = CurrentFps >= 55 ? SKColors.Lime :
                        CurrentFps >= 30 ? SKColors.Orange :
                        SKColors.Red;

            _fgPaint.Color = color;

            var text = $"FPS: {CurrentFps:F1}  (min: {MinFps:F1}  max: {MaxFps:F1})";
            if (text != _lastText)
            {
                _lastTextWidth = _font.MeasureText(text);
                _lastText = text;
            }

            canvas.DrawText(text, canvasWidth - 12 - _lastTextWidth, 20, _font, _fgPaint);
        }

        public void Reset()
        {
            _index = 0;
            _count = 0;
            _accumulator = 0;
            _elapsed = 0;
            CurrentFps = 0;
            MinFps = float.MaxValue;
            MaxFps = 0;
        }
    }
}