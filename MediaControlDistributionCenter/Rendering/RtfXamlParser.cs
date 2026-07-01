using System.Xml.Linq;

namespace MediaControlDistributionCenter.Rendering
{
    public static class RtfXamlParser
    {
        private static readonly XNamespace WpfNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        public static List<FormattedRun> Parse(string filePath)
        {
            var runs = new List<FormattedRun>();

            try
            {
                if (!File.Exists(filePath))
                    return runs;

                var doc = XDocument.Load(filePath);
                var root = doc.Root;

                if (root == null) return runs;

                var paragraphs = root.Descendants()
                    .Where(e => e.Name.LocalName == "Paragraph" ||
                                e.Name.LocalName == "Span");

                foreach (var para in paragraphs)
                {
                    foreach (var inline in para.Descendants())
                    {
                        if (inline.Name.LocalName != "Run") continue;

                        var text = inline.Value;
                        if (string.IsNullOrEmpty(text)) continue;

                        var run = new FormattedRun { Text = text };

                        var fontSizeAttr = inline.Attribute("FontSize");
                        if (fontSizeAttr != null && float.TryParse(fontSizeAttr.Value, out var fs))
                            run.FontSize = fs;

                        var fontWeightAttr = inline.Attribute("FontWeight");
                        if (fontWeightAttr != null)
                            run.IsBold = fontWeightAttr.Value.Contains("Bold", StringComparison.OrdinalIgnoreCase);

                        var fontStyleAttr = inline.Attribute("FontStyle");
                        if (fontStyleAttr != null)
                            run.IsItalic = fontStyleAttr.Value.Contains("Italic", StringComparison.OrdinalIgnoreCase);

                        var textDecorationsAttr = inline.Attribute("TextDecorations");
                        if (textDecorationsAttr != null)
                            run.IsUnderline = textDecorationsAttr.Value.Contains("Underline", StringComparison.OrdinalIgnoreCase);

                        var fontFamilyAttr = inline.Attribute("FontFamily");
                        if (fontFamilyAttr != null)
                            run.FontFamily = fontFamilyAttr.Value;

                        var foregroundAttr = inline.Attribute("Foreground");
                        if (foregroundAttr != null)
                            run.Foreground = ParseColor(foregroundAttr.Value);

                        runs.Add(run);
                    }

                    // Add newline between paragraphs
                    if (runs.Count > 0 && runs[^1].Text != "\n")
                        runs.Add(new FormattedRun { Text = "\n", FontSize = 4 });
                }

                if (runs.Count == 0)
                {
                    // Fallback: use root text content
                    var text = root.Value?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        runs.Add(new FormattedRun { Text = text });
                }
            }
            catch
            {
                // Return empty on parse failure
            }

            return runs;
        }

        public static List<FormattedRun> CreateFromPlainText(string text, float fontSize = 16f)
        {
            return new List<FormattedRun>
            {
                new FormattedRun
                {
                    Text = text,
                    FontSize = fontSize,
                    Foreground = SKColors.White,
                    IsBold = false,
                    IsItalic = false,
                }
            };
        }

        private static SKColor ParseColor(string colorValue)
        {
            try
            {
                if (string.IsNullOrEmpty(colorValue))
                    return SKColors.White;

                // Handle named colors
                colorValue = colorValue.Trim();

                // Handle #RRGGBB or #AARRGGBB
                if (colorValue.StartsWith("#"))
                {
                    var hex = colorValue.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        var r = Convert.ToByte(hex[..2], 16);
                        var g = Convert.ToByte(hex.Substring(2, 2), 16);
                        var b = Convert.ToByte(hex.Substring(4, 2), 16);
                        return new SKColor(r, g, b);
                    }
                    if (hex.Length == 8)
                    {
                        var a = Convert.ToByte(hex[..2], 16);
                        var r = Convert.ToByte(hex.Substring(2, 2), 16);
                        var g = Convert.ToByte(hex.Substring(4, 2), 16);
                        var b = Convert.ToByte(hex.Substring(6, 2), 16);
                        return new SKColor(r, g, b, a);
                    }
                }

                // Try WPF color format: #RRGGBB
                if (colorValue.Length >= 7 && colorValue[0] == '#')
                {
                    var hex = colorValue.TrimStart('#');
                    if (hex.Length >= 6)
                    {
                        var r = Convert.ToByte(hex[..2], 16);
                        var g = Convert.ToByte(hex.Substring(2, 2), 16);
                        var b = Convert.ToByte(hex.Substring(4, 2), 16);
                        return new SKColor(r, g, b);
                    }
                }
            }
            catch { }

            return SKColors.White;
        }
    }
}
