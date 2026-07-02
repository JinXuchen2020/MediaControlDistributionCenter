using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class FadeInAnimation : IAnimation
    {
        public bool IsCompleted => _elapsed >= _duration;
        public float Duration => _duration;

        private readonly float _duration;
        private float _elapsed;
        private readonly SKPaint _layerPaint = new();

        public FadeInAnimation(float durationSeconds = 0.5f)
        {
            _duration = durationSeconds;
            _layerPaint.Color = SKColors.White;
        }

        public void Update(float deltaSeconds)
        {
            if (!IsCompleted)
                _elapsed += deltaSeconds;
        }

        public void Apply(SKCanvas canvas)
        {
            float alpha = Math.Clamp(_elapsed / _duration, 0f, 1f);
            _layerPaint.Color = new SKColor(255, 255, 255, (byte)(255 * alpha));
            canvas.SaveLayer(_layerPaint);
        }
    }
}