using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaEditorSurface : IEditorSurface
    {
        private readonly SkiaCanvasController _controller;
        private readonly SkiaMouseHandler _mouseHandler;
        private MediaEditViewModel? _viewModel;

        public SkiaCanvasController Controller => _controller;
        public SkiaMouseHandler MouseHandler => _mouseHandler;

        public SkiaEditorSurface(IServiceProvider? services)
        {
            _controller = new SkiaCanvasController(services);
            var resizeHandles = services?.GetRequiredService<SkiaResizeHandles>() ?? new SkiaResizeHandles();
            _mouseHandler = new SkiaMouseHandler(_controller.RenderEngine, resizeHandles);
            _controller.RenderEngine.SurfacePool = _controller.SurfacePool;
            _controller.InitializeFactories();
        }

        public double Width
        {
            get
            {
                if (_viewModel?.CurrentMedia != null && double.TryParse(_viewModel.CurrentMedia.Width, out var mw))
                    return _controller.RenderEngine.CanvasRatio * mw;
                return 0;
            }
            set { }
        }

        public double Height
        {
            get
            {
                if (_viewModel?.CurrentMedia != null && double.TryParse(_viewModel.CurrentMedia.Height, out var mh))
                    return _controller.RenderEngine.CanvasRatio * mh;
                return 0;
            }
            set { }
        }

        public double Ratio
        {
            get => _controller.RenderEngine.CanvasRatio;
            set => _controller.RenderEngine.CanvasRatio = (float)value;
        }

        public BaseComponentViewModel? SelectedComponent => _mouseHandler.SelectedViewModel;
        public event Action<BaseComponentViewModel?>? SelectedComponentChanged;

        public void SetViewModel(MediaEditViewModel viewModel)
        {
            _viewModel = viewModel;
            _mouseHandler.ViewModel = viewModel;
        }

        public void LoadComponents(IEnumerable<BaseComponentViewModel> components)
        {
            _controller.RenderEngine.Clear();
            foreach (var component in components.Where(c => c != null && !c.IsDeleted))
            {
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
            InvalidateVisual();
        }

        public void AddComponent(BaseComponentViewModel component)
        {
            try
            {
                if (_controller.Registry.CanCreate(component.Type))
                {
                    component.CanvasRatio = _viewModel?.CanvasRatio ?? component.CanvasRatio;
                    var renderable = _controller.Registry.Create(component);
                    _controller.RenderEngine.AddRenderable(renderable);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create renderable for component type: {Type}", component.Type);
            }
        }

        public void Clear()
        {
            _controller.RenderEngine.Clear();
        }

        public byte[]? CaptureSnapshot()
        {
            var w = (float)Width;
            var h = (float)Height;
            return _controller.RenderEngine.CaptureSnapshot((int)w, (int)h);
        }

        public void InvalidateVisual()
        {
        }

        public void UpdateComponent(BaseComponentViewModel component)
        {
        }

        public void SelectComponent(BaseComponentViewModel component)
        {
            var renderable = _controller.RenderEngine.Renderables
                .FirstOrDefault(r => r.ViewModel?.Id == component.Id);
            _mouseHandler.SetSelection(renderable, component);
            UpdateSelection();
            InvalidateVisual();
        }

        public void Render(SKCanvas canvas, int width, int height)
        {
            canvas.Clear(SKColors.Black);
            _controller.RenderEngine.RenderFrame(canvas, _controller.LastDeltaSeconds);
        }

        public float UpdateDeltaTime()
        {
            return _controller.UpdateDeltaTime();
        }

        public bool NeedsRedraw => _controller.RenderEngine.NeedsRedraw;

        public void UpdateSelection()
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
            {
                var vm = selected.ViewModel;
                if (_viewModel.CurrentMedia != null)
                {
                    vm.MaxLeft = double.Parse(_viewModel.CurrentMedia.Width) - vm.Width;
                    vm.MaxTop = double.Parse(_viewModel.CurrentMedia.Height) - vm.Height;
                }
                _viewModel.SelectedComponent = null;
                _viewModel.SelectedComponent = vm;
            }
            else
            {
                _viewModel.SelectedComponent = null;
            }

            SelectedComponentChanged?.Invoke(_viewModel.SelectedComponent);
        }

        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
