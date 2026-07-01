using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class ScrollingAnimation : IAnimation
    {
        public bool IsCompleted => false;
        public float Duration => float.MaxValue;

        private readonly float _speed;
        private readonly float _startOffset;
        private readonly float _endOffset;
        private float _currentOffset;

        public ScrollingAnimation(float speed = 100f, float startOffset = 300f, float endOffset = -500f)
        {
            _speed = speed;
            _startOffset = startOffset;
            _endOffset = endOffset;
            _currentOffset = _startOffset;
        }

        public void Update(float deltaSeconds)
        {
            _currentOffset -= _speed * deltaSeconds;
            if (_currentOffset <= _endOffset)
                _currentOffset = _startOffset;
        }

        public void Apply(SKCanvas canvas)
        {
            canvas.Translate(_currentOffset, 0);
        }
    }
}
