using SkiaSharp;

namespace MediaControlDistributionCenter.Rendering
{
    public class FormattedRun
    {
        public string Text { get; set; } = string.Empty;
        public float FontSize { get; set; } = 16f;
        public SKColor Foreground { get; set; } = SKColors.White;
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string FontFamily { get; set; } = "Microsoft YaHei";
    }
}
