using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaRenderEngine
    {
        private readonly List<IRenderable> _renderables = new();
        private readonly AnimationEngine _animationEngine;
        private float _canvasRatio = 1f;

        public float CanvasRatio
        {
            get => _canvasRatio;
            set
            {
                _canvasRatio = value;
                InvalidateAll();
            }
        }

        public IReadOnlyList<IRenderable> Renderables => _renderables;

        public SkiaRenderEngine(AnimationEngine animationEngine)
        {
            _animationEngine = animationEngine;
        }

        public void AddRenderable(IRenderable renderable)
        {
            _renderables.Add(renderable);
            SortByZIndex();
        }

        public void RemoveRenderable(IRenderable renderable)
        {
            _renderables.Remove(renderable);
        }

        public void Clear()
        {
            _renderables.Clear();
        }

        public void RenderFrame(SKCanvas canvas, float deltaSeconds)
        {
            _animationEngine.Update(deltaSeconds);

            foreach (var renderable in _renderables)
            {
                if (!renderable.IsVisible)
                    continue;

                canvas.Save();
                _animationEngine.ApplyAnimations(canvas, renderable);
                renderable.Draw(canvas);
                canvas.Restore();
            }
        }

        public IRenderable? HitTest(SKPoint point)
        {
            for (int i = _renderables.Count - 1; i >= 0; i--)
            {
                if (_renderables[i].IsVisible && _renderables[i].HitTest(point))
                    return _renderables[i];
            }
            return null;
        }

        public void InvalidateAll()
        {
            foreach (var r in _renderables)
                r.Invalidate();
        }

        public byte[]? CaptureSnapshot(int width, int height)
        {
            if (_renderables.Count == 0) return null;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);

            foreach (var renderable in _renderables)
            {
                if (!renderable.IsVisible) continue;
                canvas.Save();
                renderable.Draw(canvas);
                canvas.Restore();
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            return data.ToArray();
        }

        public void PlayAnimation(IRenderable target, IAnimation animation)
        {
            _animationEngine.Play(target, animation);
        }

        private void SortByZIndex()
        {
            _renderables.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
        }

        public void Invalidate() { }
    }
}
