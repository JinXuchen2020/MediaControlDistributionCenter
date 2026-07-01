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
            _animations.Remove(target);
        }

        public void StopAll()
        {
            _animations.Clear();
        }

        public void Update(float deltaSeconds)
        {
            var completed = new List<(IRenderable, IAnimation)>();

            foreach (var (target, anims) in _animations)
            {
                foreach (var anim in anims)
                {
                    anim.Update(deltaSeconds);
                    if (anim.IsCompleted)
                        completed.Add((target, anim));
                }
            }

            foreach (var (target, anim) in completed)
            {
                _animations[target].Remove(anim);
            }

            foreach (var (target, anims) in _animations)
            {
                if (anims.Count == 0)
                    _animations.Remove(target);
            }
        }

        public void ApplyAnimations(SKCanvas canvas, IRenderable target)
        {
            if (!_animations.TryGetValue(target, out var anims))
                return;

            foreach (var anim in anims)
            {
                anim.Apply(canvas);
            }
        }
    }
}
