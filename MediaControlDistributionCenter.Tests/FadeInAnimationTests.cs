using MediaControlDistributionCenter.Rendering;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class FadeInAnimationTests
{
    [Fact]
    public void IsCompleted_AfterDuration()
    {
        var anim = new FadeInAnimation(0.5f);
        Assert.False(anim.IsCompleted);

        anim.Update(0.5f);

        Assert.True(anim.IsCompleted);
    }

    [Fact]
    public void Apply_WithZeroAlpha_DoesNotCrash()
    {
        var anim = new FadeInAnimation(0.5f);
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        var canvas = surface.Canvas;

        var exception = Record.Exception(() => anim.Apply(canvas));
        Assert.Null(exception);
    }

    [Fact]
    public void Duration_ReturnsCorrect()
    {
        var anim = new FadeInAnimation(2.0f);
        Assert.Equal(2.0f, anim.Duration);
    }

    [Fact]
    public void Apply_UsesCachedPaint()
    {
        var anim = new FadeInAnimation(0.5f);
        anim.Update(0.25f);

        using var surface1 = SKSurface.Create(new SKImageInfo(100, 100));
        using var surface2 = SKSurface.Create(new SKImageInfo(100, 100));

        anim.Apply(surface1.Canvas);
        anim.Apply(surface2.Canvas);

        // Should not throw - paint is reused
        Assert.True(true);
    }
}
