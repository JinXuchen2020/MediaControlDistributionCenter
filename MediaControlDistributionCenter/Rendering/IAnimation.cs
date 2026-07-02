using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public interface IAnimation : IDisposable
    {
        bool IsCompleted { get; }
        float Duration { get; }
        void Update(float deltaSeconds);
        void Apply(SKCanvas canvas);
    }
}
