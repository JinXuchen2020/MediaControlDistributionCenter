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
            var emptyTargets = new List<IRenderable>();

            foreach (var (target, anims) in _animations)
            {
                anims.RemoveAll(a => { a.Update(deltaSeconds); return a.IsCompleted; });
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
                anim.Apply(canvas);
            }
        }
    }
}
