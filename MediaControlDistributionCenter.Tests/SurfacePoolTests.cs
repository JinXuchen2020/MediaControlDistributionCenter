using MediaControlDistributionCenter.Rendering;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class SurfacePoolTests
{
    [Fact]
    public void GetOrCreate_ReturnsSurface()
    {
        var pool = new SurfacePool();
        var info = new SKImageInfo(100, 100);
        var surface = pool.GetOrCreate(info);
        Assert.NotNull(surface);
        pool.Dispose();
    }

    [Fact]
    public void GetOrCreate_SameSize_ReusesSurface()
    {
        var pool = new SurfacePool();
        var info = new SKImageInfo(100, 100);
        var s1 = pool.GetOrCreate(info);
        var s2 = pool.GetOrCreate(info);

        Assert.Same(s1, s2);
        pool.Dispose();
    }

    [Fact]
    public void GetOrCreate_DifferentSize_CreatesNew()
    {
        var pool = new SurfacePool();
        var s1 = pool.GetOrCreate(new SKImageInfo(100, 100));
        var s2 = pool.GetOrCreate(new SKImageInfo(200, 200));

        Assert.NotSame(s1, s2);
        pool.Dispose();
    }

    [Fact]
    public void Dispose_Cleans()
    {
        var pool = new SurfacePool();
        pool.GetOrCreate(new SKImageInfo(100, 100));
        pool.Dispose();
        // Should not throw
        Assert.True(true);
    }
}
