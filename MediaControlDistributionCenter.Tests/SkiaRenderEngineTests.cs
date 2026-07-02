using MediaControlDistributionCenter.Rendering;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class SkiaRenderEngineTests
{
    [Fact]
    public void AddRenderable_SortsByZIndex()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r1 = new TestRenderable { ZIndex = 5 };
        var r2 = new TestRenderable { ZIndex = 1 };
        var r3 = new TestRenderable { ZIndex = 3 };

        engine.AddRenderable(r1);
        engine.AddRenderable(r2);
        engine.AddRenderable(r3);

        Assert.Equal(1, engine.Renderables[0].ZIndex);
        Assert.Equal(3, engine.Renderables[1].ZIndex);
        Assert.Equal(5, engine.Renderables[2].ZIndex);
    }

    [Fact]
    public void HitTest_ReturnsTopmostFirst()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r1 = new TestRenderable { ZIndex = 1, Bounds = new SKRect(0, 0, 100, 100) };
        var r2 = new TestRenderable { ZIndex = 2, Bounds = new SKRect(0, 0, 100, 100) };

        engine.AddRenderable(r1);
        engine.AddRenderable(r2);

        var hit = engine.HitTest(new SKPoint(50, 50));
        Assert.Equal(2, hit?.ZIndex);
    }

    [Fact]
    public void HitTest_ReturnsNull_WhenNoHit()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r = new TestRenderable { Bounds = new SKRect(0, 0, 100, 100) };
        engine.AddRenderable(r);

        Assert.Null(engine.HitTest(new SKPoint(200, 200)));
    }

    [Fact]
    public void HitTest_SkipsInvisible()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r = new TestRenderable { IsVisible = false, Bounds = new SKRect(0, 0, 100, 100) };
        engine.AddRenderable(r);

        Assert.Null(engine.HitTest(new SKPoint(50, 50)));
    }

    [Fact]
    public void RemoveRenderable_DisposesAndRemoves()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r = new TestRenderable();
        engine.AddRenderable(r);

        engine.RemoveRenderable(r);

        Assert.Empty(engine.Renderables);
    }

    [Fact]
    public void Clear_DisposesAll()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        engine.AddRenderable(new TestRenderable());
        engine.AddRenderable(new TestRenderable());

        engine.Clear();

        Assert.Empty(engine.Renderables);
    }

    [Fact]
    public void RenderFrame_DoesNotCrash()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        engine.AddRenderable(new TestRenderable { Bounds = new SKRect(0, 0, 100, 100) });

        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);

        var exception = Record.Exception(() => engine.RenderFrame(canvas, 0.016f));
        Assert.Null(exception);
    }

    [Fact]
    public void CaptureSnapshot_ReturnsNull_WhenEmpty()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());

        Assert.Null(engine.CaptureSnapshot(100, 100));
    }

    [Fact]
    public void CaptureSnapshot_ReturnsData()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        engine.AddRenderable(new TestRenderable { Bounds = new SKRect(0, 0, 50, 50) });

        var data = engine.CaptureSnapshot(100, 100);

        Assert.NotNull(data);
        Assert.True(data.Length > 0);
    }

    [Fact]
    public void RemoveRenderable_DoesNotDispose()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r = new TestRenderable();
        bool disposed = false;
        r.Disposing += () => disposed = true;
        engine.AddRenderable(r);

        engine.RemoveRenderable(r);

        Assert.Empty(engine.Renderables);
        Assert.False(disposed, "RemoveRenderable should not call Dispose");
    }

    [Fact]
    public void RemoveRenderable_StopsAnimation()
    {
        var animEngine = new AnimationEngine();
        var engine = new SkiaRenderEngine(animEngine);
        var r = new TestRenderable();
        engine.AddRenderable(r);
        engine.PlayAnimation(r, new AnimationEngineTests.FakeAnimationPublic(1.0f));

        engine.RemoveRenderable(r);

        Assert.False(animEngine.HasActiveAnimations);
    }

    [Fact]
    public void Clear_StopsAllAnimations()
    {
        var animEngine = new AnimationEngine();
        var engine = new SkiaRenderEngine(animEngine);
        var r = new TestRenderable();
        engine.AddRenderable(r);
        engine.PlayAnimation(r, new AnimationEngineTests.FakeAnimationPublic(1.0f));

        engine.Clear();

        Assert.Empty(engine.Renderables);
        Assert.False(animEngine.HasActiveAnimations);
    }

    [Fact]
    public void AddRenderable_BinarySearch_InsertionOrder()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r1 = new TestRenderable { ZIndex = 5 };
        var r2 = new TestRenderable { ZIndex = 1 };
        var r3 = new TestRenderable { ZIndex = 3 };
        var r4 = new TestRenderable { ZIndex = 1 };

        engine.AddRenderable(r1);
        engine.AddRenderable(r2);
        engine.AddRenderable(r3);
        engine.AddRenderable(r4);

        Assert.Equal(1, engine.Renderables[0].ZIndex);
        Assert.Equal(1, engine.Renderables[1].ZIndex);
        Assert.Equal(3, engine.Renderables[2].ZIndex);
        Assert.Equal(5, engine.Renderables[3].ZIndex);
    }

    [Fact]
    public void RenderFrame_TracksStatistics()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        engine.AddRenderable(new TestRenderable { Bounds = new SKRect(0, 0, 100, 100), IsVisible = true });
        engine.AddRenderable(new TestRenderable { Bounds = new SKRect(0, 0, 100, 100), IsVisible = false });

        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;

        engine.RenderFrame(canvas, 0.016f);

        Assert.Equal(1, engine.Statistics.DrawCallsPerFrame);
        Assert.True(engine.Statistics.FrameTimeMs > 0);
    }

    [Fact]
    public void CanvasRatio_InvalidatesAll()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r = new TestRenderable();
        int invalidateCount = 0;
        r.OnInvalidate += () => invalidateCount++;

        engine.AddRenderable(r);
        engine.CanvasRatio = 2.0f;

        Assert.Equal(1, invalidateCount);
    }

    [Fact]
    public void IsGpuAvailable_DoesNotThrow()
    {
        var result = SkiaRenderEngine.IsGpuAvailable;
        Assert.False(result, "In CI without GPU, this should be false");
    }

    [Fact]
    public void CaptureSnapshot_WithSurfacePool()
    {
        var pool = new SurfacePool();
        var engine = new SkiaRenderEngine(new AnimationEngine());
        engine.SurfacePool = pool;
        engine.AddRenderable(new TestRenderable { Bounds = new SKRect(0, 0, 50, 50) });

        var data1 = engine.CaptureSnapshot(100, 100);
        var data2 = engine.CaptureSnapshot(100, 100);

        Assert.NotNull(data1);
        Assert.NotNull(data2);
        Assert.True(data1.Length > 0);
        Assert.True(data2.Length > 0);
    }

    [Fact]
    public void HasActiveAnimations_ReflectsState()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        Assert.False(engine.HasActiveAnimations);

        var r = new TestRenderable();
        engine.AddRenderable(r);
        engine.PlayAnimation(r, new AnimationEngineTests.FakeAnimationPublic(1.0f));

        Assert.True(engine.HasActiveAnimations);

        engine.RemoveRenderable(r);
        Assert.False(engine.HasActiveAnimations);
    }

    [Fact]
    public void DisposeRenderable_DisposesAndRemoves()
    {
        var engine = new SkiaRenderEngine(new AnimationEngine());
        var r = new TestRenderable();
        bool disposed = false;
        r.Disposing += () => disposed = true;
        engine.AddRenderable(r);

        engine.DisposeRenderable(r);

        Assert.Empty(engine.Renderables);
        Assert.True(disposed);
    }
}
