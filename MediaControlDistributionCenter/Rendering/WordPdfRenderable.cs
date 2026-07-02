using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace MediaControlDistributionCenter.Rendering
{
    public class WordPdfRenderable : IRenderable
    {
        private SKRect _bounds;
        private readonly WordComponentViewModel _vm;
        private List<SKBitmap> _pages;
        private int _currentPage;
        private DateTime _lastPageChange;

        /// <summary>
        /// Placeholder SKBitmap-based page renderer.
        /// Actual PDF/DOCX/PPTX page rendering requires PdfiumViewer or Syncfusion interop
        /// to produce SKBitmap pages. This class manages the display and page-flipping of
        /// pre-rendered page bitmaps.
        /// </summary>
        public string Type => "Word";
        public int ZIndex { get; set; }
        public SKRect Bounds => _bounds;
        public bool IsVisible { get; set; } = true;
        public BaseComponentViewModel? ViewModel => _vm;

        public WordPdfRenderable(WordComponentViewModel vm)
        {
            _vm = vm;
            ZIndex = vm.ZIndex;
            _pages = new List<SKBitmap>();
            _currentPage = 0;
            _lastPageChange = DateTime.UtcNow;
            UpdateBounds();
        }

        public void LoadPages(List<SKBitmap> pages)
        {
            foreach (var bmp in _pages)
                bmp.Dispose();
            _pages = new List<SKBitmap>(pages);
            _currentPage = 0;
        }

        public void Draw(SKCanvas canvas)
        {
            int pageDurationMs = _vm.PageDuration * 1000;
            if (pageDurationMs > 0 && (DateTime.UtcNow - _lastPageChange).TotalMilliseconds >= pageDurationMs)
            {
                NextPage();
                _lastPageChange = DateTime.UtcNow;
            }

            if (_currentPage < 0 || _currentPage >= _pages.Count)
            {
                DrawPlaceholder(canvas);
                return;
            }

            var bitmap = _pages[_currentPage];
            if (bitmap != null)
            {
                var paint = RenderResourcePool.Shared.RentPaint();
                canvas.DrawBitmap(bitmap, _bounds, paint);
                RenderResourcePool.Shared.ReturnPaint(paint);
            }
            else
            {
                DrawPlaceholder(canvas);
            }
        }

        private void DrawPlaceholder(SKCanvas canvas)
        {
            var paint = RenderResourcePool.Shared.RentPaint();
            paint.Color = new SKColor(30, 30, 30);
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(_bounds, paint);
            RenderResourcePool.Shared.ReturnPaint(paint);

            var textPaint = RenderResourcePool.Shared.RentPaint();
            textPaint.Color = SKColors.Gray;
            var textFont = RenderResourcePool.Shared.RentFont(16);
            string[] lines = string.IsNullOrEmpty(_vm.Source)
                ? new[] { "No document loaded" }
                : new[] { $"Page {_currentPage + 1}/{_pages.Count}", Path.GetFileName(_vm.Source) };
            float lineHeight = 16 * 1.4f;
            float startY = _bounds.MidY - (lines.Length - 1) * lineHeight / 2;
            for (int i = 0; i < lines.Length; i++)
            {
                canvas.DrawText(lines[i], _bounds.MidX, startY + i * lineHeight, SKTextAlign.Center, textFont, textPaint);
            }
            RenderResourcePool.Shared.ReturnFont(textFont);
            RenderResourcePool.Shared.ReturnPaint(textPaint);
        }

        public bool HitTest(SKPoint point)
        {
            return _bounds.Contains(point);
        }

        public void Invalidate()
        {
            UpdateBounds();
        }

        public void NextPage()
        {
            if (_pages.Count > 0)
                _currentPage = (_currentPage + 1) % _pages.Count;
        }

        public void PreviousPage()
        {
            if (_pages.Count > 0)
                _currentPage = (_currentPage - 1 + _pages.Count) % _pages.Count;
        }

        public void UpdateBounds()
        {
            _bounds = BoundsHelper.ComputeBounds(_vm);
        }

        public void Dispose()
        {
            foreach (var bmp in _pages)
                bmp.Dispose();
            _pages.Clear();
        }
    }
}
