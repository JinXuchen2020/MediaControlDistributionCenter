using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaControlDistributionCenter.Views.Diagrams
{
    public partial class MediaEditSkia : UserControl
    {
        private SkiaRenderEngine _renderEngine;
        private AnimationEngine _animationEngine;
        private RenderableRegistry _registry;
        private SkiaMouseHandler _mouseHandler;
        private SkiaResizeHandles _resizeHandles;
        private MediaEditViewModel? _viewModel;
        private IServiceProvider? _serviceProvider;
        private DateTime _lastFrameTime;
        private readonly FpsCounter _fpsCounter = new();

        public MediaEditSkia()
        {
            InitializeComponent();
            InitializeEngine();
        }

        public MediaEditSkia(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            InitializeEngine();
        }

        private void InitializeEngine()
        {
            _animationEngine = new AnimationEngine();
            _renderEngine = new SkiaRenderEngine(_animationEngine);
            _registry = new RenderableRegistry();
            _mouseHandler = new SkiaMouseHandler(_renderEngine);
            _resizeHandles = new SkiaResizeHandles();

            _lastFrameTime = DateTime.UtcNow;
            CompositionTarget.Rendering += OnRendering;

            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F11)
                {
                    _fpsCounter.IsVisible = !_fpsCounter.IsVisible;
                    if (_fpsCounter.IsVisible) _fpsCounter.Reset();
                    e.Handled = true;
                }
            };

            InitializeFactories();
        }

        private void InitializeFactories()
        {
            _registry.Register(new ImageComponentFactory());
            _registry.Register(new ColorTextComponentFactory());
            _registry.Register(new TextComponentFactory());
            _registry.Register(new RssComponentFactory());
            _registry.Register(new WordComponentFactory());
            _registry.Register(new VideoComponentFactory());
            _registry.Register(new WebComponentFactory());
            _registry.Register(new StreamComponentFactory());
            _registry.Register(new HdmiComponentFactory());
        }

        public void SetViewModel(MediaEditViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            if (_viewModel.CurrentMedia != null)
            {
                Dispatcher.Invoke(async () =>
                {
                    await _viewModel.LoadData();
                    LoadComponents();
                });
            }
        }

        public async void LoadComponents()
        {
            if (_viewModel?.MediaConfig?.SelectedPage == null) return;

            _renderEngine.Clear();
            _resizeHandles.SetTarget(null);

            var components = _viewModel.MediaConfig.SelectedPage.Components
                .Where(c => !c.IsDeleted);

            foreach (var component in components)
            {
                if (component == null) continue;
                try
                {
                    if (_registry.CanCreate(component.Type))
                    {
                        var renderable = _registry.Create(component);
                        _renderEngine.AddRenderable(renderable);
                    }
                }
                catch { }
            }

            SkCanvas.InvalidateVisual();
        }

        public byte[]? CaptureSnapshot()
        {
            var width = (int)_renderEngine.CanvasRatio * 768;
            var height = (int)_renderEngine.CanvasRatio * 576;
            return _renderEngine.CaptureSnapshot(width, height);
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            float deltaSeconds = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            if (deltaSeconds > 0.1f) deltaSeconds = 0.016f;
            _fpsCounter.Update(deltaSeconds);
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            float deltaSeconds = (float)(DateTime.UtcNow - _lastFrameTime).TotalSeconds;
            if (deltaSeconds > 0.1f) deltaSeconds = 0.016f;

            canvas.Clear(new SKColor(0x00, 0x00, 0x00));
            _renderEngine.RenderFrame(canvas, deltaSeconds);

            if (_mouseHandler.SelectedRenderable != null)
            {
                _resizeHandles.SetTarget(_mouseHandler.SelectedRenderable);
                _resizeHandles.Draw(canvas);
            }
            else
            {
                _resizeHandles.SetTarget(null);
            }

            _fpsCounter.Draw(canvas, (float)SkCanvas.ActualWidth);
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
            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = GetCanvasPosition(e);
            _mouseHandler.OnMouseMove(pos);

            var cursor = _mouseHandler.GetCursor(pos);
            SkCanvas.Cursor = cursor switch
            {
                CursorType.SizeAll => Cursors.SizeAll,
                CursorType.SizeWE => Cursors.SizeWE,
                CursorType.SizeNS => Cursors.SizeNS,
                CursorType.SizeNWSE => Cursors.SizeNWSE,
                CursorType.SizeNESW => Cursors.SizeNESW,
                _ => Cursors.Arrow,
            };

            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseHandler.OnMouseUp();
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
            if (_mouseHandler.SelectedRenderable != null)
            {
                _resizeHandles.SetTarget(_mouseHandler.SelectedRenderable);

                var components = _viewModel.MediaConfig?.SelectedPage?.Components;
                if (components != null)
                {
                    var bounds = _mouseHandler.SelectedRenderable.Bounds;
                    var matched = components.FirstOrDefault(c =>
                        Math.Abs(c.Left * c.Ratio - bounds.Left) < 5 &&
                        Math.Abs(c.Top * c.Ratio - bounds.Top) < 5);
                    if (matched != null)
                        _viewModel.SelectedComponent = matched;
                }
            }
            else
            {
                _resizeHandles.SetTarget(null);
                if (_viewModel != null)
                    _viewModel.SelectedComponent = null;
            }
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
            if (_serviceProvider != null && _viewModel != null)
            {
                var mainWindow = App.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
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
    }
}
