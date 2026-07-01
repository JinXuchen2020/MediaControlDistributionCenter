using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class ScrollingAnimation : IAnimation
    {
        public bool IsCompleted => _loopCount >= _maxLoops;
        public float Duration => float.MaxValue;

        private readonly float _speed;
        private readonly float _startOffset;
        private readonly float _endOffset;
        private readonly int _maxLoops;
        private float _currentOffset;
        private int _loopCount;

        public ScrollingAnimation(float speed = 100f, float startOffset = 300f, float endOffset = -500f, int maxLoops = 1)
        {
            _speed = speed;
            _startOffset = startOffset;
            _endOffset = endOffset;
            _maxLoops = maxLoops;
            _currentOffset = _startOffset;
        }

        public void Update(float deltaSeconds)
        {
            _currentOffset -= _speed * deltaSeconds;
            if (_currentOffset <= _endOffset)
            {
                _loopCount++;
                _currentOffset = _startOffset;
            }
        }

        public void Apply(SKCanvas canvas)
        {
            canvas.Translate(_currentOffset, 0);
        }
    }
}
