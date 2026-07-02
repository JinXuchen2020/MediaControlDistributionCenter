using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MediaControlDistributionCenter.Rendering
{
    public class RssRenderable : IRenderable
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        private SKRect _bounds;
        private readonly RssComponentViewModel _vm;
        private float _scrollOffset;
        private List<RssItem> _items;
        private DateTime _lastFetch;
        private volatile bool _feedLoading;
        private volatile bool _disposed;
        private int _retryCount;
        private DateTime _lastRetry = DateTime.MinValue;
        private CancellationTokenSource? _cts;
        private int _currentPage;
        private readonly object _itemsLock = new();
        private DateTime _lastScrollTime = DateTime.UtcNow;

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
        public BaseComponentViewModel? ViewModel => _vm;

        public RssRenderable(RssComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            _items = new List<RssItem>();
            _currentPage = 0;
            UpdateBounds();
            _ = FetchFeedAsync();
        }

        private async Task FetchFeedAsync()
        {
            if (_feedLoading || _disposed) return;
            if (_retryCount > 0 && !string.IsNullOrEmpty(_vm.Source))
            {
                double backoff = Math.Min(60, Math.Pow(2, _retryCount));
                if ((DateTime.UtcNow - _lastRetry).TotalSeconds < backoff) return;
            }
            _feedLoading = true;

            var fetchCts = new CancellationTokenSource();
            var token = fetchCts.Token;
            var old = Interlocked.Exchange(ref _cts, fetchCts);
            try { old?.Cancel(); } catch { }
            old?.Dispose();

            try
            {
                string url = _vm.Source ?? string.Empty;
                if (string.IsNullOrEmpty(url)) return;

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

                var response = await _httpClient.GetAsync(url, timeoutCts.Token);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                if (_disposed || token.IsCancellationRequested) return;

                var newItems = await Task.Run(() =>
                {
                    var doc = XDocument.Parse(content);
                    return doc.Descendants("item").Select(item => new RssItem
                    {
                        Title = item.Element("title")?.Value ?? string.Empty,
                        PublishDate = item.Element("pubDate")?.Value ?? string.Empty,
                        Body = item.Element("description")?.Value ?? string.Empty
                    }).ToList();
                });
                _retryCount = 0;
                _lastFetch = DateTime.UtcNow;
                lock (_itemsLock)
                    _items = newItems;
            }
            catch (OperationCanceledException)
            {
                // Expected when disposed or timeout
            }
            catch (Exception)
            {
                Interlocked.Increment(ref _retryCount);
                _lastRetry = DateTime.UtcNow;
                if (!_disposed)
                {
                    lock (_itemsLock)
                        _items = new List<RssItem>();
                }
            }
            _feedLoading = false;
        }

        public void Draw(SKCanvas canvas)
        {
            if ((DateTime.UtcNow - _lastFetch).TotalMinutes > 5 && !_feedLoading)
                _ = FetchFeedAsync();

            List<RssItem> items;
            lock (_itemsLock)
                items = _items;
            if (items.Count == 0) return;

            var bgPaint = RenderResourcePool.Shared.RentPaint();
            bgPaint.Color = new SKColor(0, 0, 0, 180);
            bgPaint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, bgPaint);
            RenderResourcePool.Shared.ReturnPaint(bgPaint);

            var contentVms = _vm.Contents;
            if (contentVms == null || contentVms.Count == 0) return;

            float y = _bounds.Top + 8;
            float x = _bounds.Left + 8;

            if (_vm.PlayMode == "rollingLeft" || _vm.PlayMode == "rollingRight")
            {
                float speed = _vm.RollingSpeed * 2f;
                float direction = _vm.PlayMode == "rollingRight" ? 1f : -1f;
                var now = DateTime.UtcNow;
                float elapsed = (float)(now - _lastScrollTime).TotalSeconds;
                _lastScrollTime = now;
                if (elapsed > 0.1f) elapsed = 0.016f;
                _scrollOffset += direction * speed * elapsed;

                if (_scrollOffset > _bounds.Width * 2 || _scrollOffset < -_bounds.Width * 2)
                    _scrollOffset = 0;

                foreach (var item in items)
                {
                    foreach (var content in contentVms)
                    {
                        string fieldValue = GetFieldValue(item, content.FieldName);
                        if (string.IsNullOrEmpty(fieldValue)) continue;

                        float fontSize = content.FontSize * (float)_vm.Ratio;
                        var paint = RenderResourcePool.Shared.RentPaint();
                        paint.Color = new SKColor(content.FontColor.R, content.FontColor.G, content.FontColor.B, content.FontColor.A);
                        var font = RenderResourcePool.Shared.RentFont(fontSize);
                        if (content.IsBold)
                            font.Typeface = RenderResourcePool.BoldTypeface;
                        if (content.IsItalic)
                            font.SkewX = -0.25f;

                        float drawX = x + _scrollOffset;
                        canvas.DrawText(fieldValue, drawX, y, font, paint);
                        y += fontSize * 1.4f;
                        RenderResourcePool.Shared.ReturnFont(font);
                        RenderResourcePool.Shared.ReturnPaint(paint);
                    }
                    y += 8;
                    if (y > _bounds.Bottom) break;
                }
            }
            else
            {
                float sumFontSize = contentVms.Sum(c => c.FontSize * 1.4f) * (float)_vm.Ratio + 8;
                int itemsPerPage = Math.Max(1, (int)((_bounds.Height - 16) / sumFontSize));
                int startIndex = _currentPage * itemsPerPage;

                for (int i = startIndex; i < items.Count && i < startIndex + itemsPerPage; i++)
                {
                    var item = items[i];
                    foreach (var content in contentVms)
                    {
                        string fieldValue = GetFieldValue(item, content.FieldName);
                        if (string.IsNullOrEmpty(fieldValue)) continue;

                        float fontSize = content.FontSize * (float)_vm.Ratio;
                        var paint = RenderResourcePool.Shared.RentPaint();
                        paint.Color = new SKColor(content.FontColor.R, content.FontColor.G, content.FontColor.B, content.FontColor.A);
                        var font = RenderResourcePool.Shared.RentFont(fontSize);
                        if (content.IsBold)
                            font.Typeface = RenderResourcePool.BoldTypeface;
                        if (content.IsItalic)
                            font.SkewX = -0.25f;

                        canvas.DrawText(fieldValue, x, y, font, paint);
                        y += fontSize * 1.4f;
                        RenderResourcePool.Shared.ReturnFont(font);
                        RenderResourcePool.Shared.ReturnPaint(paint);
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
            if (_vm.Contents == null || _vm.Contents.Count == 0) return;
            float sumFontSize = _vm.Contents.Sum(c => c.FontSize * 1.4f) * (float)_vm.Ratio + 8;
            int itemsPerPage = Math.Max(1, (int)((_bounds.Height - 16) / sumFontSize));
            int count;
            lock (_itemsLock)
                count = _items.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)count / itemsPerPage));
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
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }

        public void Dispose()
        {
            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}