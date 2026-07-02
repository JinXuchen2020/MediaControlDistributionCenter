using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public interface IRenderable : IDisposable
    {
        string Type { get; }
        int ZIndex { get; set; }
        SKRect Bounds { get; }
        bool IsVisible { get; set; }
        BaseComponentViewModel? ViewModel { get; }
        void Draw(SKCanvas canvas);
        bool HitTest(SKPoint point);
        void Invalidate();
    }
}
