using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
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
        private double maxLeft;

        [ObservableProperty]
        private double maxTop;

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

        [ObservableProperty]
        private bool isRunningLoaded;

        [ObservableProperty]
        private bool isDeleted;

        public virtual int EffectDuration => 0;

        public bool IsDragging { get; private set; }

        private Point startPoint;
        private FrameworkElement selectedElement;

        public virtual string Type { get; set; }

        public FrameworkElement? FrameworkElement { get; set; }

        public double Ratio { get; set; }

        public List<ComponentEffect> Effects => new List<ComponentEffect>
            {
                new ComponentEffect()
                {
                    Key = "Empty",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_197"),
                },
                new ComponentEffect()
                {
                    Key = "ExtendRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_154"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExtendLeft",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_155"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExtendUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_156"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExtendDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_157"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExtendOut",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_158"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExtendLeftRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_159"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExtendUpDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_160"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "MoveRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_161"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "MoveLeft",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_162"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "MoveUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_163"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "MoveDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_164"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ZipRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_165"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ZipLeft",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_166"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ZipUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_167"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ZipDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_168"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ZipUpDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_169"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ZipLeftRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_170"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ScrollUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_171"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ScrollDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_172"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "BlindH",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_173"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "BlindV",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_174"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "FullScreen",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_175"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "Wheel",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_176"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "Engage",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_177"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "FadeIn",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_153"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "StackRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_178"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "StackLeft",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_179"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "StackUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_180"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "StackDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_181"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "LaserRight",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_182"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "LaserLeft",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_183"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "LaserUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_184"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "LaserDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_185"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExpandDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_186"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExpandUp",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_187"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "ExpandUpDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_188"),
                    Action = FadeIn
                },
                new ComponentEffect()
                {
                    Key = "MergeUpDown",
                    Name = FindResource("LanguageKey_Code_ProgramEdit_Tooltip_189"),
                    Action = FadeIn
                },
            };

        public BaseComponentViewModel()
        {
        }

        public BaseComponentViewModel(BaseComponent component, string userAccount, double ratio)
        {
            Id = component.Id;
            Name = component.Name;
            ZIndex = component.ZIndex;
            Type = component.Type.ToString();
            Left = component.Left;
            Top = component.Top;
            Width = component.Width;
            Height = component.Height;
            timeline = component.Timeline;
            playCount = component.PlayCount;
            playDuration = component.PlayDuration;
            Ratio = ratio;

            switch (component.Type)
            {
                case Models.MediaType.Video:
                    isFile = true;
                    source = string.IsNullOrEmpty(component.Source) ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, userAccount, component.Source);
                    fileName = string.IsNullOrEmpty(component.Source) ? null : Path.GetFileName(source);
                    isShowInfo = !string.IsNullOrEmpty(component.Source);
                    break;
                case Models.MediaType.Image:
                    isFile = true;
                    source = string.IsNullOrEmpty(component.Source) ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, userAccount, component.Source);
                    fileName = string.IsNullOrEmpty(component.Source) ? null : Path.GetFileName(source);
                    isShowInfo = !string.IsNullOrEmpty(component.Source);
                    break;
                case Models.MediaType.Word:
                    isFile = true;
                    source = string.IsNullOrEmpty(component.Source) ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, userAccount, component.Source);
                    fileName = string.IsNullOrEmpty(component.Source) ? null : Path.GetFileName(source);
                    isShowInfo = !string.IsNullOrEmpty(component.Source);
                    break;
                default:
                    isFile = false;
                    source = component.Source;
                    isShowInfo = true;
                    break;
            }
        }

        public virtual BaseComponent ToModel(string userAccount, double ratio)
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
                if (element != null)
                {
                    canvas.Children.Add(element);
                }
            }
        }

        [RelayCommand]
        private void Dispose()
        {
            if (IsFile && !string.IsNullOrEmpty(Source) && FrameworkElement != null)
            {
                DisposeContent();
                FrameworkElement = null;
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

        protected virtual void FadeIn(FrameworkElement element)
        {
            return;
        }

        public virtual void EffectExecution()
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
                startPoint = e.GetPosition(canvas);
                selectedElement.CaptureMouse(); // 捕获鼠标
                var resizableControl = new ResizableControl();
                if(manageViewModel.SelectedElement != null)
                {
                    resizableControl.ClearResizable(manageViewModel.SelectedElement, canvas);
                    manageViewModel.SelectedElement = null;
                }

                if (manageViewModel.SelectedComponent != null)
                {
                    manageViewModel.SelectedComponent.IsSelected = false;
                    manageViewModel.SelectedComponent = null;
                }
                var viewModel = selectedElement.DataContext as BaseComponentViewModel;
                viewModel.MaxLeft = double.Parse(manageViewModel.CurrentMedia.Width) - viewModel.Width;
                viewModel.MaxTop = double.Parse(manageViewModel.CurrentMedia.Height) - viewModel.Height;
                manageViewModel.SelectedComponent = viewModel;
                manageViewModel.SelectedComponent.IsSelected = true;
                manageViewModel.SelectedElement = selectedElement;
                resizableControl.MakeResizable(selectedElement, canvas);
            }
        }

        // 图片鼠标左键释放时触发
        protected void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragging && e.ChangedButton == MouseButton.Left)
            {
                selectedElement.ReleaseMouseCapture(); // 释放鼠标
                IsDragging = false;
                selectedElement = null;
            }
        }

        // 图片鼠标移动时触发
        protected void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                IsDragging = true;
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

                var mediaEditViewModel = App.ServicesProvider.GetRequiredService<MediaEditViewModel>();
                Left = newLeft / Ratio / mediaEditViewModel.CanvasRatio;
                Top = newTop / Ratio / mediaEditViewModel.CanvasRatio;

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

        protected static string FindResource(string key)
        {
            return (string)LanguageTool.Instance.FindResource(key);
        }

        protected void CreateBinding(DependencyObject element, DependencyProperty dp, string path,  IValueConverter? converter = null, object? converterParameter = null)
        {
            var binding = new Binding(path)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };

            if (converter != null)
            {
                binding.Converter = converter;
                binding.ConverterParameter = converterParameter;
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

        protected Border CreateBorder(UIElement child)
        {
            Border result = new()
            {
                BorderThickness = new Thickness(4),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30479C")),
                Width = Width,
                Height = Height,
                DataContext = this,
                Child = child
            };

            var mediaEditViewModel = App.ServicesProvider.GetRequiredService<MediaEditViewModel>();
            var converter = new ToMultipleConverter();
            CreateBinding(result, FrameworkElement.WidthProperty, nameof(Width), converter, Ratio * mediaEditViewModel.CanvasRatio);
            CreateBinding(result, FrameworkElement.HeightProperty, nameof(Height), converter, Ratio * mediaEditViewModel.CanvasRatio);

            Canvas.SetLeft(result, Left * Ratio * mediaEditViewModel.CanvasRatio);
            Canvas.SetTop(result, Top * Ratio * mediaEditViewModel.CanvasRatio);
            Canvas.SetZIndex(result, ZIndex);

            // 添加鼠标事件处理
            result.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            result.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            result.MouseMove += Element_MouseMove;
            result.MouseWheel += Element_MouseWheel;

            //var rectangle = new System.Windows.Shapes.Rectangle
            //{
            //    StrokeDashArray = new DoubleCollection([4, 2]),
            //    Stroke = Brushes.Gray,
            //    StrokeThickness = 2,
            //};

            //var binding = new Binding("Width")
            //{
            //    RelativeSource = new RelativeSource
            //    {
            //        AncestorType = typeof(Border)
            //    }
            //};

            //rectangle.SetBinding(System.Windows.Shapes.Rectangle.WidthProperty, binding);
            //var heightBinding = new Binding("Height")
            //{
            //    RelativeSource = new RelativeSource
            //    {
            //        AncestorType = typeof(Border)
            //    }
            //};

            //rectangle.SetBinding(System.Windows.Shapes.Rectangle.HeightProperty, heightBinding);

            //VisualBrush borderBrush = new VisualBrush()
            //{
            //    Visual = rectangle
            //};
            //result.BorderBrush = borderBrush;

            return result;
        }
        
        protected IList<FontFamily> LoadFonts()
        {
            // 创建InstalledFontCollection对象
            ICollection<FontFamily> fontFamilies = Fonts.SystemFontFamilies;
            return fontFamilies.ToList();
        }
    }

    public class ComponentEffect
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public Action<FrameworkElement> Action { get; set; }
    }
}
