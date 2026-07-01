using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class ColorTextRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;
        private readonly ColorTextComponentViewModel _colorVm;
        private readonly SKPaint _textPaint;
        private readonly SKShader _gradientShader;
        private readonly SKImageFilter _dropShadow;

        public string Type => "ColorText";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;

        public ColorTextRenderable(ColorTextComponentViewModel vm)
        {
            _vm = vm;
            _colorVm = vm;
            ZIndex = vm.ZIndex;
            UpdateBounds();

            _gradientShader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(1, 1),
                new[] { SKColors.Red, SKColors.Orange, SKColors.Yellow },
                new float[] { 0f, 0.5f, 1f },
                SKShaderTileMode.Clamp);

            _dropShadow = SKImageFilter.CreateDropShadow(
                2f, 2f, 5f, 5f, new SKColor(128, 0, 128, 128));

            _textPaint = new SKPaint
            {
                IsAntialias = true,
                SubpixelText = true,
                Color = SKColors.White,
                Shader = _gradientShader,
                ImageFilter = _dropShadow,
            };
        }

        public void Draw(SKCanvas canvas)
        {
            _textPaint.TextSize = (float)(_colorVm.FontSize * _vm.Ratio);

            string text = _colorVm.Source ?? string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                canvas.DrawText(text, _bounds.Left, _bounds.Top + _textPaint.TextSize, _textPaint);
            }
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            _bounds = new SKRect(
                (float)(_vm.Left * _vm.Ratio),
                (float)(_vm.Top * _vm.Ratio),
                (float)((_vm.Left + _vm.Width) * _vm.Ratio),
                (float)((_vm.Top + _vm.Height) * _vm.Ratio));
        }

        public void Dispose()
        {
            _textPaint?.Dispose();
            _gradientShader?.Dispose();
            _dropShadow?.Dispose();
        }
    }
}