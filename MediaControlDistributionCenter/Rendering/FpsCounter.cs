using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class FpsCounter
    {
        private readonly Queue<float> _frameTimes = new(120);
        private float _elapsed;
        public float CurrentFps { get; private set; }
        public float MinFps { get; private set; } = float.MaxValue;
        public float MaxFps { get; private set; }
        public bool IsVisible { get; set; }

        public void Update(float deltaSeconds)
        {
            _frameTimes.Enqueue(deltaSeconds);
            _elapsed += deltaSeconds;

            if (_elapsed >= 0.5f)
            {
                var avg = _frameTimes.Average();
                CurrentFps = avg > 0 ? 1f / avg : 0;
                if (CurrentFps < MinFps) MinFps = CurrentFps;
                if (CurrentFps > MaxFps) MaxFps = CurrentFps;
                _elapsed = 0;
                while (_frameTimes.Count > 120) _frameTimes.Dequeue();
            }
        }

        public void Draw(SKCanvas canvas, float canvasWidth)
        {
            if (!IsVisible) return;

            using var bg = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 140),
                Style = SKPaintStyle.Fill,
            };
            var bgRect = new SKRect(canvasWidth - 240, 2, canvasWidth - 4, 28);
            canvas.DrawRoundRect(new SKRoundRect(bgRect, 4), bg);

            var color = CurrentFps >= 55 ? SKColors.Lime :
                        CurrentFps >= 30 ? SKColors.Orange :
                        SKColors.Red;

            using var paint = new SKPaint
            {
                Color = color,
                TextSize = 14,
                IsAntialias = true,
            };

            var text = $"FPS: {CurrentFps:F1}  (min: {MinFps:F1}  max: {MaxFps:F1})";
            canvas.DrawText(text, canvasWidth - 12 - paint.MeasureText(text), 20, paint);
        }

        public void Reset()
        {
            _frameTimes.Clear();
            _elapsed = 0;
            CurrentFps = 0;
            MinFps = float.MaxValue;
            MaxFps = 0;
        }
    }
}
