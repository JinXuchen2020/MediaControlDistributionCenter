using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class AnimationEngine
    {
        private readonly Dictionary<IRenderable, List<IAnimation>> _animations = new();

        public void Play(IRenderable target, IAnimation animation)
        {
            if (!_animations.ContainsKey(target))
                _animations[target] = new List<IAnimation>();
            _animations[target].Add(animation);
        }

        public void Stop(IRenderable target)
        {
            if (_animations.TryGetValue(target, out var anims))
            {
                foreach (var anim in anims)
                    anim.Dispose();
                _animations.Remove(target);
            }
        }

        public void StopAll()
        {
            foreach (var anims in _animations.Values)
                foreach (var anim in anims)
                    anim.Dispose();
            _animations.Clear();
            if (_animations.Capacity > 16)
                _animations.TrimExcess();
        }

        public bool HasActiveAnimations => _animations.Count > 0;

        public int ActiveFadeInCount
        {
            get
            {
                int count = 0;
                foreach (var anims in _animations.Values)
                {
                    foreach (var anim in anims)
                    {
                        if (anim is FadeInAnimation fade && !fade.IsCompleted)
                            count++;
                    }
                }
                return count;
            }
        }

        public float MaxFadeInAlpha
        {
            get
            {
                float maxAlpha = 1f;
                foreach (var anims in _animations.Values)
                {
                    foreach (var anim in anims)
                    {
                        if (anim is FadeInAnimation fade && !fade.IsCompleted)
                        {
                            float alpha = Math.Clamp(fade.Elapsed / fade.Duration, 0f, 1f);
                            if (alpha < maxAlpha) maxAlpha = alpha;
                        }
                    }
                }
                return maxAlpha;
            }
        }

        public void Update(float deltaSeconds)
        {
            var emptyTargets = new List<IRenderable>();

            foreach (var (target, anims) in _animations)
            {
                var completed = new List<IAnimation>();
                anims.RemoveAll(a =>
                {
                    a.Update(deltaSeconds);
                    if (a.IsCompleted)
                        completed.Add(a);
                    return a.IsCompleted;
                });
                foreach (var c in completed)
                    c.Dispose();
                if (anims.Count == 0)
                    emptyTargets.Add(target);
            }

            foreach (var target in emptyTargets)
                _animations.Remove(target);
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
