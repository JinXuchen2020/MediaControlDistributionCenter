using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class ResizableControl
    {
        private Canvas _canvas;
        private ResizeDirection _resizeDirection;

        private static Dictionary<FrameworkElement, List<Thumb>> _elementToThumbMap = new Dictionary<FrameworkElement, List<Thumb>>();

        public void MakeResizable(FrameworkElement control, Canvas canvas)
        {
            _canvas = canvas;

            // 创建四个调整大小的区域
            CreateResizeArea(control, ResizeDirection.TopLeft);
            CreateResizeArea(control, ResizeDirection.TopRight);
            CreateResizeArea(control, ResizeDirection.BottomLeft);
            CreateResizeArea(control, ResizeDirection.BottomRight);
            CreateResizeArea(control, ResizeDirection.Left);
            CreateResizeArea(control, ResizeDirection.Right);
            CreateResizeArea(control, ResizeDirection.Bottom);
            CreateResizeArea(control, ResizeDirection.Top);

            UpdateThumbPositions(control);
        }

        public void ClearResizable(FrameworkElement control, Canvas canvas)
        {
            _canvas = canvas;

            if (_elementToThumbMap.ContainsKey(control))
            {
                foreach(var item in _elementToThumbMap[control])
                {
                    _canvas.Children.Remove(item);
                }
            }
        }

        private void CreateResizeArea(FrameworkElement control, ResizeDirection direction)
        {
            var thumb = new Thumb
            {
                Width = 10,
                Height = 10,
                Background = Brushes.Transparent,
                Tag = direction
            };

            _canvas.Children.Add(thumb);
            thumb.DragStarted += (s, e) =>
            {
                _resizeDirection = direction;
            };

            thumb.DragDelta += (s, e) =>
            {
                System.Threading.Thread.Sleep(50);
                var deltaX = e.HorizontalChange;
                var deltaY = e.VerticalChange;

                var newLeft = Canvas.GetLeft(control);
                var newTop = Canvas.GetTop(control);
                var newWidth = control.Width;
                var newHeight = control.Height;

                double ratio = newWidth / newHeight;

                switch (_resizeDirection)
                {
                    case ResizeDirection.TopLeft:
                        newWidth = Math.Max(control.Width - deltaX, 10);
                        newHeight = Math.Max(control.Height - deltaY, 10);
                        if (Math.Abs(deltaX) > Math.Abs(deltaY))
                        {
                            newHeight = newWidth / ratio;
                        }
                        else
                        {
                            newWidth = newHeight * ratio;
                        }
                        newLeft = Math.Max(newLeft + (control.Width - newWidth), 0);
                        newTop = Math.Max(newTop + (control.Height - newHeight), 0);                        
                        break;
                    case ResizeDirection.TopRight:
                        newWidth = Math.Max(control.Width + deltaX, 10);
                        newHeight = Math.Max(control.Height - deltaY, 10);
                        if (Math.Abs(deltaX) > Math.Abs(deltaY))
                        {
                            newHeight = newWidth / ratio;
                        }
                        else
                        {
                            newWidth = newHeight * ratio;
                        }
                        newTop = Math.Max(newTop + (control.Height - newHeight), 0);
                        break;
                    case ResizeDirection.BottomLeft:
                        newWidth = Math.Max(control.Width - deltaX, 10);
                        newHeight = Math.Max(control.Height + deltaY, 10);
                        if (Math.Abs(deltaX) > Math.Abs(deltaY))
                        {
                            newHeight = newWidth / ratio;
                        }
                        else
                        {
                            newWidth = newHeight * ratio;
                        }
                        newLeft = Math.Max(newLeft + (control.Width - newWidth), 0);
                        break;
                    case ResizeDirection.BottomRight:
                        newWidth = Math.Max(control.Width + deltaX, 10);
                        newHeight = Math.Max(control.Height + deltaY, 10);
                        if (Math.Abs(deltaX) > Math.Abs(deltaY))
                        {
                            newHeight = newWidth / ratio;
                        }
                        else
                        {
                            newWidth = newHeight * ratio;
                        }
                        break;
                    case ResizeDirection.Left:
                        newWidth = Math.Max(control.Width - deltaX, 10);
                        newLeft = Math.Max(newLeft + (control.Width - newWidth), 0);
                        break;
                    case ResizeDirection.Right:
                        newWidth = Math.Max(control.Width + deltaX, 10);
                        break;
                    case ResizeDirection.Top:
                        newHeight = Math.Max(control.Height - deltaY, 10);
                        newTop = Math.Max(newTop + (control.Height - newHeight), 0);
                        break;
                    case ResizeDirection.Bottom:
                        newHeight = Math.Max(control.Height + deltaY, 10);
                        break;                 
                }

                control.Width = Math.Min(newWidth, _canvas.Width - newLeft);
                control.Height = Math.Min(newHeight, _canvas.Height - newTop);
                Canvas.SetLeft(control, newLeft);
                Canvas.SetTop(control, newTop);
                ((BaseComponentViewModel)control.DataContext).Left = newLeft;
                ((BaseComponentViewModel)control.DataContext).Top = newTop;
                UpdateThumbPositions(control);
            };
            if(_elementToThumbMap.ContainsKey(control))
            {
                _elementToThumbMap[control].Add(thumb);
            }
            else
            {
                _elementToThumbMap.Add(control, new List<Thumb>() { thumb });
            }
        }

        private void UpdateThumbPositions(FrameworkElement control)
        {
            var conLeft = Canvas.GetLeft(control);
            var conTop = Canvas.GetTop(control);
            var zIndex = Canvas.GetZIndex(control);
            var width = control.ActualWidth;
            var height = control.ActualHeight;
            var offsetX = (control.Width - width)/2;
            var offsetY = (control.Height - height)/ 2;

            if (_elementToThumbMap.ContainsKey(control))
            {
                foreach (var thumb in _elementToThumbMap[control])
                {
                    var direction = thumb.Tag;
                    switch (direction)
                    {
                        case ResizeDirection.TopLeft:
                            thumb.Cursor = Cursors.SizeNWSE;
                            Canvas.SetLeft(thumb, conLeft + offsetX);
                            Canvas.SetTop(thumb, conTop + offsetY);
                            break;
                        case ResizeDirection.TopRight:
                            thumb.Cursor = Cursors.SizeNESW;
                            Canvas.SetLeft(thumb, conLeft + width + offsetX - thumb.Width);
                            Canvas.SetTop(thumb, conTop + offsetY);
                            break;
                        case ResizeDirection.BottomLeft:
                            thumb.Cursor = Cursors.SizeNESW;
                            Canvas.SetLeft(thumb, conLeft + offsetX);
                            Canvas.SetTop(thumb, conTop + height + offsetY - thumb.Height);
                            break;
                        case ResizeDirection.BottomRight:
                            thumb.Cursor = Cursors.SizeNWSE;
                            Canvas.SetLeft(thumb, conLeft + width + offsetX - thumb.Width);
                            Canvas.SetTop(thumb, conTop + height + offsetY - thumb.Height);
                            break;
                        case ResizeDirection.Left:
                            thumb.Cursor = Cursors.SizeWE;
                            thumb.Height = 20;
                            Canvas.SetLeft(thumb, conLeft + offsetX);
                            Canvas.SetTop(thumb, conTop + height / 2 + offsetY);
                            break;
                        case ResizeDirection.Right:
                            thumb.Cursor = Cursors.SizeWE;
                            thumb.Height = 20;
                            Canvas.SetLeft(thumb, conLeft + width + offsetX - thumb.Width);
                            Canvas.SetTop(thumb, conTop + height / 2 + offsetY);
                            break;
                        case ResizeDirection.Top:
                            thumb.Cursor = Cursors.SizeNS;
                            thumb.Width = 20;
                            Canvas.SetLeft(thumb, conLeft + width / 2 + offsetX);
                            Canvas.SetTop(thumb, conTop + offsetY);
                            break;
                        case ResizeDirection.Bottom:
                            thumb.Cursor = Cursors.SizeNS;
                            thumb.Width = 20;
                            Canvas.SetLeft(thumb, conLeft + width / 2 + offsetX - thumb.Width);
                            Canvas.SetTop(thumb, conTop + height + offsetY - thumb.Height);
                            break;
                    }

                    Canvas.SetZIndex(thumb, zIndex);
                }
            }
        }
    }

    public enum ResizeDirection
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top,
        Bottom,
        Left,
        Right
    }
}
