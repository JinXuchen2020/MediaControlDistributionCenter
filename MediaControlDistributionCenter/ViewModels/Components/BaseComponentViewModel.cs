using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Views.CustomControls;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class BaseComponentViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int zIndex;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isFile;

        [ObservableProperty]
        private string? fileName;

        [ObservableProperty]
        private string? source;

        [ObservableProperty]
        private double left;

        [ObservableProperty]
        private double top;

        [ObservableProperty]
        private double width;

        [ObservableProperty]
        private double height;

        [ObservableProperty]
        private double timeline;

        [ObservableProperty]
        private int playCount;

        [ObservableProperty]
        private string playDuration;

        [ObservableProperty]
        private bool isShowInfo;

        private bool isDragging = false;
        private Point startPoint;
        private FrameworkElement selectedElement;

        public virtual string Type { get; set; }

        public FrameworkElement? FrameworkElement { get; set; }

        public double Ratio { get; set; }

        public BaseComponentViewModel()
        {

        }

        public BaseComponentViewModel(BaseComponent component, double ratio)
        {
            Id = component.Id;
            Name = component.Name;
            ZIndex = component.ZIndex;
            Type = component.Type.ToString();
            Left = component.Left * ratio;
            Top = component.Top * ratio;
            Width = component.Width * ratio;
            Height = component.Height * ratio;
            timeline = component.Timeline;
            playCount = component.PlayCount;
            playDuration = component.PlayDuration;

            switch (component.Type)
            {
                case Models.MediaType.Video:
                    isFile = true;
                    source = string.IsNullOrEmpty(component.Source) ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, component.Source);
                    fileName = string.IsNullOrEmpty(component.Source) ? null : Path.GetFileName(source);
                    isShowInfo = !string.IsNullOrEmpty(component.Source);
                    break;
                case Models.MediaType.Image:
                    isFile = true;
                    source = string.IsNullOrEmpty(component.Source) ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, component.Source);
                    fileName = string.IsNullOrEmpty(component.Source) ? null : Path.GetFileName(source);
                    isShowInfo = !string.IsNullOrEmpty(component.Source);
                    break;
                case Models.MediaType.Text:
                    isFile = false;
                    source = component.Source;
                    isShowInfo = true;
                    break;
            }
        }

        public virtual BaseComponent ToModel(double ratio)
        {
            return new BaseComponent();
        }

        [RelayCommand]
        private void DrawContent(Canvas canvas)
        {
            if (!string.IsNullOrEmpty(Source))
            {
                var element = DrawingContent();
                canvas.Children.Add(element);
            }
        }

        [RelayCommand]
        private void DrawRunningContent(Canvas canvas)
        {
            if (!string.IsNullOrEmpty(Source))
            {
                var element = DrawingRunningContent();
                canvas.Children.Add(element);
            }
        }

        [RelayCommand]
        private void Dispose()
        {
            if (IsFile && !string.IsNullOrEmpty(Source) && FrameworkElement != null)
            {
                DisposeContent();
            }
        }

        protected virtual FrameworkElement DrawingContent()
        {
            return default;
        }

        protected virtual FrameworkElement DrawingRunningContent()
        {
            return default;
        }

        protected virtual void DisposeContent()
        {
            return;
        }

        protected void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedElement = sender as FrameworkElement;
            if (selectedElement != null && e.ChangedButton == MouseButton.Left)
            {
                var canvas = FindCanvasParent(selectedElement);
                var manageViewModel = (canvas.DataContext as MediaEditViewModel)!;
                isDragging = true;
                startPoint = e.GetPosition(canvas);
                selectedElement.CaptureMouse(); // 捕获鼠标
                manageViewModel.SelectedElement = null;
                if (manageViewModel.SelectedComponent!= null)
                {
                    manageViewModel.SelectedComponent.IsSelected = false;
                }
                var viewModel = selectedElement.DataContext as BaseComponentViewModel;
                manageViewModel.SelectedComponent = viewModel;
                manageViewModel.SelectedComponent.IsSelected = true;
                manageViewModel.SelectedElement = selectedElement;
                var resizableControl = new ResizableControl();
                resizableControl.MakeResizable(selectedElement, canvas);
            }
        }

        // 图片鼠标左键释放时触发
        protected void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && e.ChangedButton == MouseButton.Left)
            {
                selectedElement.ReleaseMouseCapture(); // 释放鼠标
                isDragging = false;
                selectedElement = null;
            }
        }

        // 图片鼠标移动时触发
        protected void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = FindCanvasParent(selectedElement);
                Point currentPoint = e.GetPosition(canvas);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                double newLeft = Canvas.GetLeft(selectedElement);
                double newTop = Canvas.GetTop(selectedElement);
                double maxLeft = canvas.Width - selectedElement.Width;
                double maxTop = canvas.Height - selectedElement.Height;

                var resizableControl = new ResizableControl();
                resizableControl.ClearResizable(selectedElement, canvas);

                newLeft = Math.Min(Math.Max(newLeft + offsetX, 0), maxLeft);
                newTop = Math.Min(Math.Max(newTop + offsetY, 0), maxTop);

                // 更新图片位置
                Canvas.SetLeft(selectedElement, newLeft);
                Canvas.SetTop(selectedElement, newTop);

                resizableControl.MakeResizable(selectedElement, canvas);

                Left = newLeft;
                Top = newTop;

                startPoint = currentPoint;
            }
        }

        // 图片鼠标滚轮滚动时触发
        protected void Element_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (selectedElement != null)
            {
                double scaleFactor = e.Delta > 0 ? 1.1 : 0.9; // 缩放比例

                // 更新图片大小
                selectedElement.Width *= scaleFactor;
                selectedElement.Height *= scaleFactor;
            }
        }

        protected Canvas FindCanvasParent(Visual child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            while (parentObject != null)
            {
                if (parentObject is Canvas)
                {
                    return parentObject as Canvas;
                }

                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null; // 如果没有找到Canvas，则返回null
        }

        protected void CreateBinding(DependencyObject element, DependencyProperty dp, string path,  IValueConverter? converter = null)
        {
            var binding = new Binding(path)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };

            if (converter != null)
            {
                binding.Converter = converter;
            }

            if (element is FrameworkContentElement contentElement)
            {
                contentElement.SetBinding(dp, binding);
                return;
            }

            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.SetBinding(dp, binding);
                return;
            }
        }
    }
}
