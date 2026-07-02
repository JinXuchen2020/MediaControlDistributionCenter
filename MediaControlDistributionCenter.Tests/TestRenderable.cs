using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Tests;

public class TestRenderable : IRenderable
{
    public string Type => "Test";
    public int ZIndex { get; set; }
    public SKRect Bounds { get; set; }
    public bool IsVisible { get; set; } = true;
    public BaseComponentViewModel? ViewModel => null;

    public event Action? Disposing;
    public event Action? OnInvalidate;

    public void Draw(SKCanvas canvas) { }
    public bool HitTest(SKPoint point) => Bounds.Contains(point);
    public void Invalidate() => OnInvalidate?.Invoke();
    public void Dispose() => Disposing?.Invoke();
}
