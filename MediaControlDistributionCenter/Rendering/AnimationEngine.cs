using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class AnimationEngine
    {
        private readonly Dictionary<IRenderable, List<IAnimation>> _animations = new();
        internal static AnimationEngine? Global { get; set; }
        private int _activeFadeInCount;
        private float _maxFadeInAlpha = 1f;
        private readonly List<IRenderable> _emptyTargetBuffer = new();
        private readonly List<IAnimation> _completedBuffer = new();

        public void Play(IRenderable target, IAnimation animation)
        {
            if (!_animations.ContainsKey(target))
                _animations[target] = new List<IAnimation>();
            _animations[target].Add(animation);
            RecalcFadeInAlpha();
        }

        public void Stop(IRenderable target)
        {
            if (_animations.TryGetValue(target, out var anims))
            {
                foreach (var anim in anims)
                    anim.Dispose();
                _animations.Remove(target);
            }
            RecalcFadeInAlpha();
        }

        public void StopAll()
        {
            foreach (var anims in _animations.Values)
                foreach (var anim in anims)
                    anim.Dispose();
            _animations.Clear();
            RecalcFadeInAlpha();
        }

        public bool HasActiveAnimations => _animations.Count > 0;

        public int ActiveFadeInCount => _activeFadeInCount;

        public float MaxFadeInAlpha => _maxFadeInAlpha;

        private void RecalcFadeInAlpha()
        {
            _activeFadeInCount = 0;
            _maxFadeInAlpha = 1f;
            foreach (var (_, animList) in _animations)
            {
                foreach (var a in animList)
                {
                    if (a is FadeInAnimation fade && !fade.IsCompleted)
                    {
                        _activeFadeInCount++;
                        float alpha = Math.Clamp(fade.Elapsed / fade.Duration, 0f, 1f);
                        if (alpha < _maxFadeInAlpha) _maxFadeInAlpha = alpha;
                    }
                }
            }
        }

        public void Update(float deltaSeconds)
        {
            _emptyTargetBuffer.Clear();

            foreach (var (target, anims) in _animations)
            {
                _completedBuffer.Clear();
                anims.RemoveAll(a =>
                {
                    a.Update(deltaSeconds);
                    if (a.IsCompleted)
                        _completedBuffer.Add(a);
                    return a.IsCompleted;
                });
                foreach (var c in _completedBuffer)
                    c.Dispose();
                if (anims.Count == 0)
                    _emptyTargetBuffer.Add(target);
            }

            foreach (var target in _emptyTargetBuffer)
                _animations.Remove(target);

            RecalcFadeInAlpha();
        }

        public void ApplyAnimations(SKCanvas canvas, IRenderable target)
        {
            if (!_animations.TryGetValue(target, out var anims))
                return;

            foreach (var anim in anims)
            {
                try
                {
                    anim.Apply(canvas);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "ApplyAnimations: exception in animation for {Type}", target.Type);
                }
            }
        }
    }
}
