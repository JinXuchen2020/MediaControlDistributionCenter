using MediaControlDistributionCenter.Rendering;
using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Linq;
using System.Windows;
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
        private DateTime _lastFrameTime;

        public MediaEditSkia()
        {
            InitializeComponent();

            _animationEngine = new AnimationEngine();
            _renderEngine = new SkiaRenderEngine(_animationEngine);
            _registry = new RenderableRegistry();
            _mouseHandler = new SkiaMouseHandler(_renderEngine);
            _resizeHandles = new SkiaResizeHandles();

            _lastFrameTime = DateTime.UtcNow;
            CompositionTarget.Rendering += OnRendering;

            InitializeFactories();
        }

        private void InitializeFactories()
        {
            _registry.Register(new ImageComponentFactory());
            _registry.Register(new ColorTextComponentFactory());
        }

        public void SetViewModel(MediaEditViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            LoadComponents();
        }

        public void LoadComponents()
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

        private void OnRendering(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            float deltaSeconds = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;

            if (deltaSeconds > 0.1f) deltaSeconds = 0.016f;

            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            float deltaSeconds = (float)(DateTime.UtcNow - _lastFrameTime).TotalSeconds;
            if (deltaSeconds > 0.1f) deltaSeconds = 0.016f;

            canvas.Clear(new SKColor(0x00, 0x00, 0x00));

            _renderEngine.RenderFrame(canvas, deltaSeconds);

            _resizeHandles.Draw(canvas);
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

            if (_viewModel != null)
            {
                _resizeHandles.SetTarget(_mouseHandler.SelectedRenderable);
                if (_mouseHandler.SelectedRenderable != null)
                {
                    _renderEngine.AddRenderable(_resizeHandles);
                }
            }

            SkCanvas.InvalidateVisual();
        }

        private void SkCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _mouseHandler.OnMouseWheel(e.Delta);
            SkCanvas.InvalidateVisual();
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
                    // File drop handling - placeholder for now
                    // Will be connected to the existing file handling logic
                    LoadComponents();
                }
            }
        }
    }
}
