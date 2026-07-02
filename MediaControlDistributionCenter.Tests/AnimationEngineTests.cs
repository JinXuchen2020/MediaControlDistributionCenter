using MediaControlDistributionCenter.Rendering;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class AnimationEngineTests
{
    [Fact]
    public void Play_AddsAnimation()
    {
        var engine = new AnimationEngine();
        var renderable = new TestRenderable();
        var anim = new FakeAnimationPublic(1.0f);

        engine.Play(renderable, anim);

        Assert.False(anim.IsCompleted);
    }

    [Fact]
    public void Update_RemovesCompletedAnimation()
    {
        var engine = new AnimationEngine();
        var renderable = new TestRenderable();
        var anim = new FakeAnimationPublic(0.5f);

        engine.Play(renderable, anim);
        engine.Update(1.0f);

        Assert.True(anim.IsCompleted);
    }

    [Fact]
    public void Stop_RemovesAllAnimationsForTarget()
    {
        var engine = new AnimationEngine();
        var renderable = new TestRenderable();
        engine.Play(renderable, new FakeAnimationPublic(1.0f));
        engine.Play(renderable, new FakeAnimationPublic(2.0f));

        engine.Stop(renderable);
        engine.Update(1.0f);

        // No animations to apply - nothing should crash
        var canvas = new SKCanvas(new SKBitmap(100, 100));
        engine.ApplyAnimations(canvas, renderable);
    }

    [Fact]
    public void StopAll_ClearsAll()
    {
        var engine = new AnimationEngine();
        var r1 = new TestRenderable();
        engine.Play(r1, new FakeAnimationPublic(1.0f));
        engine.Play(new TestRenderable(), new FakeAnimationPublic(2.0f));

        engine.StopAll();

        var canvas = new SKCanvas(new SKBitmap(100, 100));
        engine.ApplyAnimations(canvas, r1);
        // Should not apply anything after stop
    }

    public class FakeAnimationPublic : IAnimation
    {
        public bool IsCompleted { get; private set; }
        public float Duration { get; }
        private float _elapsed;

        public FakeAnimationPublic(float duration)
        {
            Duration = duration;
        }

        public void Update(float deltaSeconds)
        {
            _elapsed += deltaSeconds;
            if (_elapsed >= Duration)
                IsCompleted = true;
        }

        public void Apply(SKCanvas canvas) { }
    }
}
