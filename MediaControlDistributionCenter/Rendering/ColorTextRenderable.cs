using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class ColorTextRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly BaseComponentViewModel _vm;
        private readonly ColorTextComponentViewModel _colorVm;

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
        }

        public void Draw(SKCanvas canvas)
        {
            using var textPaint = new SKPaint
            {
                TextSize = (float)(_colorVm.FontSize * _vm.Ratio),
                IsAntialias = true,
                SubpixelText = true,
                Color = SKColors.White
            };

            using var gradientShader = SKShader.CreateLinearGradient(
                new SKPoint(_bounds.Left, _bounds.Top),
                new SKPoint(_bounds.Right, _bounds.Bottom),
                new[] { SKColors.Red, SKColors.Orange, SKColors.Yellow },
                new float[] { 0f, 0.5f, 1f },
                SKShaderTileMode.Clamp);

            textPaint.Shader = gradientShader;

            using var dropShadow = SKImageFilter.CreateDropShadow(
                2f, 2f, 5f, 5f, new SKColor(128, 0, 128, 128));

            textPaint.ImageFilter = dropShadow;

            string text = _colorVm.Source ?? string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                canvas.DrawText(text, _bounds.Left, _bounds.Top + textPaint.TextSize, textPaint);
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
    }
}
