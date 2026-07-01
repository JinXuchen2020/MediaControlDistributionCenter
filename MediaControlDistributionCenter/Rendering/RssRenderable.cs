using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MediaControlDistributionCenter.Rendering
{
    public class RssRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly RssComponentViewModel _vm;
        private float _scrollOffset;
        private List<RssItem> _items;
        private DateTime _lastFetch;
        private int _currentPage;

        private class RssItem
        {
            public string Title { get; set; }
            public string PublishDate { get; set; }
            public string Body { get; set; }
        }

        public string Type => "Rss";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;

        public RssRenderable(RssComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            _items = new List<RssItem>();
            _currentPage = 0;
            UpdateBounds();
            FetchFeed();
        }

        private void FetchFeed()
        {
            try
            {
                string url = _vm.Source ?? string.Empty;
                if (string.IsNullOrEmpty(url)) return;

                var doc = XDocument.Load(url);
                _items = doc.Descendants("item").Select(item => new RssItem
                {
                    Title = item.Element("title")?.Value ?? string.Empty,
                    PublishDate = item.Element("pubDate")?.Value ?? string.Empty,
                    Body = item.Element("description")?.Value ?? string.Empty
                }).ToList();
            }
            catch
            {
                _items = new List<RssItem>();
            }
            _lastFetch = DateTime.UtcNow;
        }

        public void Draw(SKCanvas canvas)
        {
            if ((DateTime.UtcNow - _lastFetch).TotalMinutes > 5)
                FetchFeed();

            if (_items.Count == 0) return;

            using var bgPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 180),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            };
            canvas.DrawRect(_bounds, bgPaint);

            var contentVms = _vm.Contents;
            if (contentVms == null || contentVms.Count == 0) return;

            float y = _bounds.Top + 8;
            float x = _bounds.Left + 8;

            if (_vm.PlayMode == "rollingLeft" || _vm.PlayMode == "rollingRight")
            {
                float speed = _vm.RollingSpeed * 2f;
                float direction = _vm.PlayMode == "rollingRight" ? 1f : -1f;
                _scrollOffset += direction * speed;

                foreach (var item in _items)
                {
                    foreach (var content in contentVms)
                    {
                        string fieldValue = GetFieldValue(item, content.FieldName);
                        if (string.IsNullOrEmpty(fieldValue)) continue;

                        float fontSize = content.FontSize * (float)_vm.Ratio;
                        using var textPaint = new SKPaint
                        {
                            TextSize = fontSize,
                            IsAntialias = true,
                            SubpixelText = true,
                            Color = new SKColor(content.FontColor.R, content.FontColor.G, content.FontColor.B, content.FontColor.A),
                            IsBold = content.IsBold,
                            FakeBoldText = content.IsBold,
                            TextSkewX = content.IsItalic ? -0.25f : 0f,
                        };

                        float drawX = x + _scrollOffset;
                        canvas.DrawText(fieldValue, drawX, y, textPaint);
                        y += fontSize * 1.4f;
                    }
                    y += 8;
                    if (y > _bounds.Bottom) break;
                }
            }
            else
            {
                int itemsPerPage = Math.Max(1, (int)((_bounds.Height - 16) / (contentVms.Sum(c => c.FontSize * 1.4f) * (float)_vm.Ratio + 8)));
                int startIndex = _currentPage * itemsPerPage;

                for (int i = startIndex; i < _items.Count && i < startIndex + itemsPerPage; i++)
                {
                    var item = _items[i];
                    foreach (var content in contentVms)
                    {
                        string fieldValue = GetFieldValue(item, content.FieldName);
                        if (string.IsNullOrEmpty(fieldValue)) continue;

                        float fontSize = content.FontSize * (float)_vm.Ratio;
                        using var textPaint = new SKPaint
                        {
                            TextSize = fontSize,
                            IsAntialias = true,
                            SubpixelText = true,
                            Color = new SKColor(content.FontColor.R, content.FontColor.G, content.FontColor.B, content.FontColor.A),
                            IsBold = content.IsBold,
                            FakeBoldText = content.IsBold,
                            TextSkewX = content.IsItalic ? -0.25f : 0f,
                        };

                        canvas.DrawText(fieldValue, x, y, textPaint);
                        y += fontSize * 1.4f;
                    }
                    y += 8;
                    if (y > _bounds.Bottom) break;
                }
            }
        }

        private static string GetFieldValue(RssItem item, string fieldName)
        {
            return fieldName switch
            {
                "Title" => item.Title,
                "PublishDate" => item.PublishDate,
                "Body" => item.Body,
                _ => string.Empty
            };
        }

        public void NextPage()
        {
            int itemsPerPage = _vm.Contents?.Count > 0
                ? Math.Max(1, (int)((_bounds.Height - 16) / (_vm.Contents.Sum(c => c.FontSize * 1.4f) * (float)_vm.Ratio + 8)))
                : 1;
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_items.Count / itemsPerPage));
            _currentPage = (_currentPage + 1) % totalPages;
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            _bounds = new SKRect(
                (float)(_vm.Left * _vm.Ratio),
                (float)(_vm.Top * _vm.Ratio),
                (float)((_vm.Left + _vm.Width) * _vm.Ratio),
                (float)((_vm.Top + _vm.Height) * _vm.Ratio));
        }
    }
}
