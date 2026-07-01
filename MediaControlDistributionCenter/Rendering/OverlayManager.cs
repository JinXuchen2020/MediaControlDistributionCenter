using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class OverlayEntry
    {
        public IRenderable Renderable { get; set; }
        public UIElement Element { get; set; }
        public BaseComponentViewModel ViewModel { get; set; }
        public bool IsOverlay { get; set; }
    }

    public class OverlayManager
    {
        private readonly Canvas _hostCanvas;
        private readonly List<OverlayEntry> _entries = new();
        private SkiaRenderEngine? _renderEngine;

        public OverlayManager(Canvas hostCanvas)
        {
            _hostCanvas = hostCanvas;
        }

        public void SetRenderEngine(SkiaRenderEngine engine)
        {
            _renderEngine = engine;
        }

        public void AddOverlay(IRenderable renderable, UIElement element, BaseComponentViewModel vm, bool isOverlay = true)
        {
            var entry = new OverlayEntry
            {
                Renderable = renderable,
                Element = element,
                ViewModel = vm,
                IsOverlay = isOverlay
            };
            _entries.Add(entry);

            if (!_hostCanvas.Children.Contains(element))
                _hostCanvas.Children.Add(element);
        }

        public void RemoveOverlay(IRenderable renderable)
        {
            var entry = _entries.Find(e => e.Renderable == renderable);
            if (entry != null)
            {
                if (_hostCanvas.Children.Contains(entry.Element))
                    _hostCanvas.Children.Remove(entry.Element);
                _entries.Remove(entry);
            }
        }

        public void SyncPositions(float canvasScale = 1f)
        {
            foreach (var entry in _entries)
            {
                if (entry.Renderable.IsVisible)
                {
                    var bounds = entry.Renderable.Bounds;
                    Canvas.SetLeft(entry.Element, bounds.Left);
                    Canvas.SetTop(entry.Element, bounds.Top);
                    Canvas.SetZIndex(entry.Element, entry.Renderable.ZIndex + 1000);
                    entry.Element.Visibility = Visibility.Visible;

                    if (entry.Element is FrameworkElement fe)
                    {
                        fe.Width = bounds.Width;
                        fe.Height = bounds.Height;
                    }
                }
                else
                {
                    entry.Element.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void Clear()
        {
            foreach (var entry in _entries)
            {
                if (_hostCanvas.Children.Contains(entry.Element))
                    _hostCanvas.Children.Remove(entry.Element);
            }
            _entries.Clear();
        }

        public IReadOnlyList<OverlayEntry> Entries => _entries;
    }
}
