using SkiaSharp;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class FadeInAnimation : IAnimation
    {
        public bool IsCompleted => _elapsed >= _duration;
        public float Duration => _duration;

        private readonly float _duration;
        private float _elapsed;
        private readonly SKPaint _layerPaint;
        public float Elapsed => _elapsed;

        public FadeInAnimation(float durationSeconds = 0.5f)
        {
            _duration = durationSeconds;
            _layerPaint = new SKPaint { Color = SKColors.White };
        }

        public void Update(float deltaSeconds)
        {
            if (!IsCompleted)
                _elapsed += deltaSeconds;
        }

        public void Apply(SKCanvas canvas)
        {
        }

        public void Dispose()
        {
            _layerPaint?.Dispose();
        }
    }
}