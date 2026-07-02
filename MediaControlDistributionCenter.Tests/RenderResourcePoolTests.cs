using MediaControlDistributionCenter.Rendering;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class RenderResourcePoolTests
{
    [Fact]
    public void RentPaint_ReturnsValidPaint()
    {
        var pool = new RenderResourcePool();
        var paint = pool.RentPaint();
        Assert.NotNull(paint);
        Assert.True(paint.IsAntialias);
        pool.ReturnPaint(paint);
    }

    [Fact]
    public void RentReturn_Cycle_ReusesObjects()
    {
        var pool = new RenderResourcePool(maxPerType: 4);
        var p1 = pool.RentPaint();
        pool.ReturnPaint(p1);

        var p2 = pool.RentPaint();
        Assert.Same(p1, p2);
        pool.ReturnPaint(p2);
    }

    [Fact]
    public void Rent_MoreThanMax_CreatesNew()
    {
        var pool = new RenderResourcePool(maxPerType: 1);
        var p1 = pool.RentPaint();
        var p2 = pool.RentPaint();

        Assert.NotNull(p1);
        Assert.NotNull(p2);
        Assert.NotSame(p1, p2);

        pool.ReturnPaint(p1);
        pool.ReturnPaint(p2);

        // Second return should be disposed (pool full)
        var p3 = pool.RentPaint();
        Assert.NotNull(p3);
        pool.ReturnPaint(p3);
    }

    [Fact]
    public void RentFont_WithSize()
    {
        var pool = new RenderResourcePool();
        var font = pool.RentFont(14f);
        Assert.NotNull(font);
        Assert.Equal(14f, font.Size);
        pool.ReturnFont(font);
    }

    [Fact]
    public void RentFont_Returns_SizeIsReset()
    {
        var pool = new RenderResourcePool(maxPerType: 4);
        var f1 = pool.RentFont(14f);
        pool.ReturnFont(f1);

        var f2 = pool.RentFont(20f);
        Assert.Same(f1, f2);
        Assert.Equal(20f, f2.Size);
        pool.ReturnFont(f2);
    }

    [Fact]
    public void Dispose_CleansAll()
    {
        var pool = new RenderResourcePool();
        pool.RentPaint();
        pool.RentPaint();
        pool.RentFont(12f);

        pool.Dispose();
        // Should not throw
        Assert.True(true);
    }
}
