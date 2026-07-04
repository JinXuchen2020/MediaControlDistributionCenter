using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.CustomControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.Rendering
{
    public class WpfEditorSurface : IEditorSurface
    {
        private readonly Canvas _canvas;
        private MediaEditViewModel? _viewModel;
        private FrameworkElement? _selectedElement;

        public WpfEditorSurface(Canvas canvas)
        {
            _canvas = canvas;
        }

        public double Width
        {
            get => _canvas.Width;
            set => _canvas.Width = value;
        }

        public double Height
        {
            get => _canvas.Height;
            set => _canvas.Height = value;
        }

        public double Ratio { get; set; }

        public BaseComponentViewModel? SelectedComponent { get; private set; }
        public event Action<BaseComponentViewModel?>? SelectedComponentChanged;

        public void SetViewModel(MediaEditViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void LoadComponents(IEnumerable<BaseComponentViewModel> components)
        {
            _canvas.Children.Clear();
            foreach (var component in components.Where(c => c != null && !c.IsDeleted))
            {
                component.DrawContentCommand.Execute(_canvas);
            }
            InvalidateVisual();
        }

        public void Clear()
        {
            _canvas.Children.Clear();
            _selectedElement = null;
            SelectedComponent = null;
        }

        public byte[]? CaptureSnapshot()
        {
            return _canvas.Dispatcher.Invoke(() =>
            {
                var renderTargetBitmap = new RenderTargetBitmap(
                    (int)_canvas.Width, (int)_canvas.Height, 96, 96, PixelFormats.Pbgra32);
                _canvas.Measure(new Size(_canvas.Width, _canvas.Height));
                _canvas.Arrange(new Rect(new Size(_canvas.Width, _canvas.Height)));
                renderTargetBitmap.Render(_canvas);

                var png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using var memoryStream = new MemoryStream();
                png.Save(memoryStream);
                return memoryStream.ToArray();
            });
        }

        public void InvalidateVisual()
        {
            _canvas.InvalidateVisual();
        }

        public void UpdateComponent(BaseComponentViewModel component)
        {
        }

        public void AddComponent(BaseComponentViewModel component)
        {
            component.DrawContentCommand.Execute(_canvas);
        }

        public void RemoveComponent(BaseComponentViewModel component)
        {
            if (component.FrameworkElement != null)
            {
                _canvas.Children.Remove(component.FrameworkElement);
                component.FrameworkElement = null;
            }
        }

        public void ClearSelection()
        {
            if (_selectedElement != null)
            {
                var resizableControl = new ResizableControl();
                resizableControl.ClearResizable(_selectedElement, _canvas);
            }
            _selectedElement = null;
            SelectedComponent = null;
            SelectedComponentChanged?.Invoke(null);
        }

        public void SelectComponent(BaseComponentViewModel component)
        {
            SelectedComponent = component;
            SelectedComponentChanged?.Invoke(component);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
