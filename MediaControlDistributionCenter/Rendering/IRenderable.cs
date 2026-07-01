using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public interface IRenderable
    {
        string Type { get; }
        int ZIndex { get; set; }
        SKRect Bounds { get; }
        bool IsVisible { get; set; }
        void Draw(SKCanvas canvas);
        bool HitTest(SKPoint point);
        void Invalidate();
    }
}
