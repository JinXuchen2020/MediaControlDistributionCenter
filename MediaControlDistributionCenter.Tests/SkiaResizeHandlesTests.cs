using MediaControlDistributionCenter.Rendering;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class SkiaResizeHandlesTests
{
    [Fact]
    public void SetTarget_MakesVisible()
    {
        var handles = new SkiaResizeHandles();
        Assert.False(handles.IsVisible);

        handles.SetTarget(new TestRenderable { Bounds = new SKRect(0, 0, 100, 100) });
        Assert.True(handles.IsVisible);
    }

    [Fact]
    public void SetTarget_Null_MakesInvisible()
    {
        var handles = new SkiaResizeHandles();
        handles.SetTarget(new TestRenderable { Bounds = new SKRect(0, 0, 100, 100) });
        handles.SetTarget(null);

        Assert.False(handles.IsVisible);
    }

    [Fact]
    public void HitTestHandleIndex_ReturnsMinusOne_WhenInvisible()
    {
        var handles = new SkiaResizeHandles();
        var result = handles.HitTestHandleIndex(new SKPoint(50, 50));

        Assert.Equal(-1, result);
    }

    [Fact]
    public void HitTestHandleIndex_FindsHandle()
    {
        var handles = new SkiaResizeHandles();
        handles.SetTarget(new TestRenderable { Bounds = new SKRect(0, 0, 100, 100) });

        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;
        handles.Draw(canvas);

        int index = handles.HitTestHandleIndex(new SKPoint(0, 0));
        Assert.True(index >= 0, "Handle at (0,0) should be found");
    }

    [Fact]
    public void ZIndex_IsMaxValue()
    {
        var handles = new SkiaResizeHandles();
        Assert.Equal(int.MaxValue, handles.ZIndex);
    }

    [Fact]
    public void Draw_DoesNotCrash_WithNoTarget()
    {
        var handles = new SkiaResizeHandles();
        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;

        var exception = Record.Exception(() => handles.Draw(canvas));
        Assert.Null(exception);
    }
}
