using MediaControlDistributionCenter.ViewModels;
using SkiaSharp;
using System;
using System.Linq;
using System.Windows.Input;

namespace MediaControlDistributionCenter.Rendering
{
    public class SkiaMouseHandler
    {
        private readonly SkiaRenderEngine _engine;
        private IRenderable? _selectedRenderable;
        private SKPoint _lastMousePoint;
        private bool _isDragging;
        private bool _isResizing;
        private int _resizeHandleIndex = -1;
        private BaseComponentViewModel? _selectedVm;

        public IRenderable? SelectedRenderable => _selectedRenderable;
        public BaseComponentViewModel? SelectedViewModel => _selectedVm;

        public bool IsLeftButtonPressed { get; set; }
        public SKPoint CurrentPosition { get; set; }

        public MediaEditViewModel? ViewModel { get; set; }

        public SkiaMouseHandler(SkiaRenderEngine engine)
        {
            _engine = engine;
        }

        private BaseComponentViewModel? FindComponent(IRenderable renderable)
        {
            if (ViewModel?.MediaConfig?.SelectedPage == null) return null;
            var bounds = renderable.Bounds;
            return ViewModel.MediaConfig.SelectedPage.Components
                .FirstOrDefault(c => !c.IsDeleted &&
                    Math.Abs(c.Left * c.Ratio - bounds.Left) < 5 &&
                    Math.Abs(c.Top * c.Ratio - bounds.Top) < 5);
        }

        public void OnMouseDown(SKPoint position)
        {
            _lastMousePoint = position;
            CurrentPosition = position;
            IsLeftButtonPressed = true;

            if (_selectedRenderable != null)
            {
                _resizeHandleIndex = HitTestResizeHandles(position);
                if (_resizeHandleIndex >= 0)
                {
                    _isResizing = true;
                    _selectedVm = FindComponent(_selectedRenderable);
                    return;
                }
            }

            var hit = _engine.HitTest(position);
            if (hit != null)
            {
                _selectedRenderable = hit;
                _selectedVm = FindComponent(hit);
                _isDragging = true;
            }
            else
            {
                _selectedRenderable = null;
                _selectedVm = null;
                _isDragging = false;
            }
        }

        public void OnMouseMove(SKPoint position)
        {
            CurrentPosition = position;
            var delta = new SKPoint(
                position.X - _lastMousePoint.X,
                position.Y - _lastMousePoint.Y);

            float ratio = _selectedVm?.Ratio ?? 1f;

            if (_isDragging && _selectedVm != null)
            {
                _selectedVm.Left += delta.X / ratio;
                _selectedVm.Top += delta.Y / ratio;
                _selectedRenderable?.Invalidate();
            }
            else if (_isResizing && _selectedVm != null)
            {
                float dx = delta.X / ratio;
                float dy = delta.Y / ratio;

                switch (_resizeHandleIndex)
                {
                    case 0: _selectedVm.Left += dx; _selectedVm.Top += dy; _selectedVm.Width -= dx; _selectedVm.Height -= dy; break;
                    case 1: _selectedVm.Top += dy; _selectedVm.Width += dx; _selectedVm.Height -= dy; break;
                    case 2: _selectedVm.Left += dx; _selectedVm.Width -= dx; _selectedVm.Height += dy; break;
                    case 3: _selectedVm.Width += dx; _selectedVm.Height += dy; break;
                    case 4: _selectedVm.Left += dx; _selectedVm.Width -= dx; break;
                    case 5: _selectedVm.Width += dx; break;
                    case 6: _selectedVm.Top += dy; _selectedVm.Height -= dy; break;
                    case 7: _selectedVm.Height += dy; break;
                }

                if (_selectedVm.Width < 10) _selectedVm.Width = 10;
                if (_selectedVm.Height < 10) _selectedVm.Height = 10;
                _selectedRenderable?.Invalidate();
            }

            _lastMousePoint = position;
        }

        public void OnMouseUp()
        {
            IsLeftButtonPressed = false;
            _isDragging = false;
            _isResizing = false;
            _resizeHandleIndex = -1;
        }

        public void OnMouseWheel(int delta, float zoomFactor = 0.1f)
        {
            if (delta > 0)
                _engine.CanvasRatio += zoomFactor;
            else
                _engine.CanvasRatio = Math.Max(0.1f, _engine.CanvasRatio - zoomFactor);
        }

        public CursorType GetCursor(SKPoint position)
        {
            if (_selectedRenderable != null)
            {
                int handle = HitTestResizeHandles(position);
                if (handle >= 0)
                    return GetResizeCursor(handle);
                if (_selectedRenderable.HitTest(position))
                    return CursorType.SizeAll;
            }
            return CursorType.Arrow;
        }

        private int HitTestResizeHandles(SKPoint position)
        {
            if (_selectedRenderable == null) return -1;
            var bounds = _selectedRenderable.Bounds;
            float handleSize = 10f;

            // 8 handles: TL, TR, BL, BR, L, R, T, B
            SKPoint[] handlePositions = new[]
            {
                new SKPoint(bounds.Left, bounds.Top),                          // TL
                new SKPoint(bounds.Right, bounds.Top),                         // TR
                new SKPoint(bounds.Left, bounds.Bottom),                       // BL
                new SKPoint(bounds.Right, bounds.Bottom),                      // BR
                new SKPoint(bounds.Left, bounds.MidY),                         // L
                new SKPoint(bounds.Right, bounds.MidY),                        // R
                new SKPoint(bounds.MidX, bounds.Top),                          // T
                new SKPoint(bounds.MidX, bounds.Bottom),                       // B
            };

            for (int i = 0; i < handlePositions.Length; i++)
            {
                var rect = new SKRect(
                    handlePositions[i].X - handleSize,
                    handlePositions[i].Y - handleSize,
                    handlePositions[i].X + handleSize,
                    handlePositions[i].Y + handleSize);
                if (rect.Contains(position))
                    return i;
            }
            return -1;
        }

        private CursorType GetResizeCursor(int handle)
        {
            return handle switch
            {
                0 => CursorType.SizeNWSE,    // TL
                1 => CursorType.SizeNESW,    // TR
                2 => CursorType.SizeNESW,    // BL
                3 => CursorType.SizeNWSE,    // BR
                4 => CursorType.SizeWE,      // L
                5 => CursorType.SizeWE,      // R
                6 => CursorType.SizeNS,      // T
                7 => CursorType.SizeNS,      // B
                _ => CursorType.Arrow,
            };
        }

        public void ClearSelection()
        {
            _selectedRenderable = null;
            _selectedVm = null;
            _isDragging = false;
            _isResizing = false;
        }
    }

    public enum CursorType
    {
        Arrow,
        SizeAll,
        SizeWE,
        SizeNS,
        SizeNWSE,
        SizeNESW
    }
}
