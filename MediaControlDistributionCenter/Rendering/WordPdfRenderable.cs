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
                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                };
                canvas.DrawBitmap(bitmap, _bounds, paint);
            }
            else
            {
                DrawPlaceholder(canvas);
            }
        }

        private void DrawPlaceholder(SKCanvas canvas)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(30, 30, 30),
                Style = SKPaintStyle.Fill,
            };
            canvas.DrawRect(_bounds, paint);

            using var textPaint = new SKPaint
            {
                TextSize = 16,
                IsAntialias = true,
                Color = SKColors.Gray,
                TextAlign = SKTextAlign.Center,
            };
            string label = string.IsNullOrEmpty(_vm.Source)
                ? "No document loaded"
                : $"Page {_currentPage + 1}/{_pages.Count}\n{Path.GetFileName(_vm.Source)}";
            canvas.DrawText(label,
                _bounds.MidX,
                _bounds.MidY,
                textPaint);
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
            _bounds = new SKRect(
                (float)(_vm.Left * _vm.Ratio),
                (float)(_vm.Top * _vm.Ratio),
                (float)((_vm.Left + _vm.Width) * _vm.Ratio),
                (float)((_vm.Top + _vm.Height) * _vm.Ratio));
        }

        public void Dispose()
        {
            foreach (var bmp in _pages)
                bmp.Dispose();
            _pages.Clear();
        }
    }
}
