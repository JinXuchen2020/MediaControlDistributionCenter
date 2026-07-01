using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public interface IAnimation
    {
        bool IsCompleted { get; }
        float Duration { get; }
        void Update(float deltaSeconds);
        void Apply(SKCanvas canvas);
    }
}
