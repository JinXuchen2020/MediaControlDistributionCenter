using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class ScrollAnimation : IAnimation
    {
        public bool IsCompleted => false;
        public float Duration => float.MaxValue;

        private readonly float _boundsWidth;
        private readonly float _totalWidth;
        private readonly float _speed;
        private readonly float _direction;
        private readonly bool _loopEnabled;
        private float _scrollOffset;

        public float ScrollOffset => _scrollOffset;

        public ScrollAnimation(float boundsWidth, float totalWidth, float speed, float direction, bool loopEnabled)
        {
            _boundsWidth = boundsWidth;
            _totalWidth = totalWidth;
            _speed = speed * 1.5f;
            _direction = direction;
            _loopEnabled = loopEnabled;
        }

        public void Update(float deltaSeconds)
        {
            _scrollOffset += _direction * _speed * deltaSeconds;

            if (_loopEnabled)
            {
                if (_scrollOffset > _boundsWidth)
                    _scrollOffset -= _boundsWidth + _totalWidth;
                if (_scrollOffset < -(_totalWidth + _boundsWidth))
                    _scrollOffset += _boundsWidth + _totalWidth;
            }
            else
            {
                _scrollOffset = Math.Clamp(_scrollOffset, -(_totalWidth + 10), _boundsWidth);
            }
        }

        public void Apply(SKCanvas canvas)
        {
        }

        public void Dispose()
        {
        }
    }
}
