using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    public partial class MediaEditSkia : UserControl
    {
        private SkiaCanvasController _controller;
        private SkiaMouseHandler _mouseHandler;
        private MediaEditViewModel? _viewModel;
        private IServiceProvider? _serviceProvider;

        public MediaEditSkia()
        {
            InitializeComponent();
            this.Unloaded += MediaEditSkia_Unloaded;
            InitializeEngine();
        }

        public MediaEditSkia(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            this.Unloaded += MediaEditSkia_Unloaded;
            InitializeEngine();
        }

        private void InitializeEngine()
        {
            _controller = new SkiaCanvasController(_serviceProvider);
            var resizeHandles = _serviceProvider != null
                ? _serviceProvider.GetRequiredService<SkiaResizeHandles>()
                : new SkiaResizeHandles();
            _mouseHandler = new SkiaMouseHandler(_controller.RenderEngine, resizeHandles);

            CompositionTarget.Rendering += OnRendering;
            this.PreviewKeyDown += OnPreviewKeyDown;
            SkCanvas.PaintSurface += SkCanvas_PaintSurface;

            _controller.RenderEngine.SurfacePool = _controller.SurfacePool;
            _controller.InitializeFactories();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                _controller.ToggleFps();
                e.Handled = true;
            }
        }

        private void MediaEditSkia_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_controller == null) return;
            _controller.RenderEngine.IsInteracting = false;
            CompositionTarget.Rendering -= OnRendering;
            this.PreviewKeyDown -= OnPreviewKeyDown;
            SkCanvas.PaintSurface -= SkCanvas_PaintSurface;
            _controller.Dispose();
        }

        private void SkCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Clear(new SKColor(0x00, 0x00, 0x00));
            _controller.RenderEngine.RenderFrame(canvas, _controller.LastDeltaSeconds);

            _controller.FpsCounter.Draw(canvas, (float)SkCanvas.ActualWidth, _controller.RenderEngine.Statistics);
        }

        public void SetViewModel(MediaEditViewModel viewModel)
        {
            _viewModel = viewModel;
            _mouseHandler.ViewModel = viewModel;
            DataContext = viewModel;

            if (_viewModel.CurrentMedia != null)
            {
                Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        await _viewModel.LoadData();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "LoadData failed");
                    }
                    LoadComponents();
                });
            }
        }

        public async void LoadComponents()
        {
            try
            {
                if (_viewModel?.SelectedPage == null) return;

                _controller.RenderEngine.Clear();

                var components = _viewModel.SelectedPage.Components
                    .Where(c => !c.IsDeleted);

                foreach (var component in components)
                {
                    if (component == null) continue;
                    try
                    {
                        if (_controller.Registry.CanCreate(component.Type))
                        {
                            var renderable = _controller.Registry.Create(component);
                            _controller.RenderEngine.AddRenderable(renderable);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to create renderable for component type: {Type}", component.Type);
                    }
                }

                SkCanvas.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load components");
            }
        }

        public byte[]? CaptureSnapshot()
        {
            var width = (int)_controller.RenderEngine.CanvasRatio * 768;
            var height = (int)_controller.RenderEngine.CanvasRatio * 576;
            return _controller.RenderEngine.CaptureSnapshot(width, height);
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            _controller.UpdateDeltaTime();
            _controller.FpsCounter.Update(_controller.LastDeltaSeconds);

            if (_controller.RenderEngine.NeedsRedraw)
            {
                SkCanvas.InvalidateVisual();
            }
        }

        private SKPoint GetCanvasPosition(MouseEventArgs e)
        {
            var pos = e.GetPosition(SkCanvas);
            return new SKPoint((float)pos.X, (float)pos.Y);
        }

        private void SkCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = GetCanvasPosition(e);
            _mouseHandler.OnMouseDown(pos);
            UpdateSelection();
            _controller.RenderEngine.IsInteracting = true;
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = GetCanvasPosition(e);
            _mouseHandler.OnMouseMove(pos);

            var cursor = _mouseHandler.GetCursor(pos);
            SkCanvas.Cursor = cursor switch
            {
                Rendering.CursorType.SizeAll => Cursors.SizeAll,
                Rendering.CursorType.SizeWE => Cursors.SizeWE,
                Rendering.CursorType.SizeNS => Cursors.SizeNS,
                Rendering.CursorType.SizeNWSE => Cursors.SizeNWSE,
                Rendering.CursorType.SizeNESW => Cursors.SizeNESW,
                _ => Cursors.Arrow,
            };

            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseHandler.OnMouseUp();
            _controller.RenderEngine.IsInteracting = false;
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _mouseHandler.OnMouseWheel(e.Delta);
            SkCanvas.InvalidateVisual();
        }

        private void UpdateSelection()
        {
            if (_viewModel == null) return;
            var engine = _controller.RenderEngine;
            var resizeHandles = _mouseHandler.ResizeHandles;

            var selected = _mouseHandler.SelectedRenderable;
            if (selected != null)
            {
                if (!engine.Renderables.Contains(resizeHandles))
                {
                    resizeHandles.SetTarget(selected);
                    engine.AddRenderable(resizeHandles);
                }
            }
            else
            {
                if (engine.Renderables.Contains(resizeHandles))
                {
                    engine.RemoveRenderable(resizeHandles);
                    resizeHandles.SetTarget(null);
                }
            }

            if (selected?.ViewModel != null)
                _viewModel.SelectedComponent = selected.ViewModel;
            else
                _viewModel.SelectedComponent = null;
        }

        private void SkCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void SkCanvas_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void SkCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && _viewModel != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadComponents();
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_serviceProvider == null) return;

            var mainWindow = App.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            if (_viewModel.ShowNavigation)
            {
                var content = _serviceProvider.GetRequiredService<MediaManage>();
                mainWindow.GoContent(content, 2);
            }
            else
            {
                var content = _serviceProvider.GetRequiredService<UserControllers>();
                mainWindow.GoContent(content, 2);
            }
        }
    }
}
