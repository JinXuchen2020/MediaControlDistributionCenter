using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.Xaml.Behaviors.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class ImageComponentViewModel : BaseComponentViewModel
    {
        [ObservableProperty]
        private int effectDuration;  //特效时长    -毫秒

        [ObservableProperty]
        private string componentEffect; //"上下展开",

        [ObservableProperty]
        private string componentEffectKey;

        private FrameworkElement RunningElement;

        private DispatcherTimer? _timer;
        private int currentPlayCount = 0;

        public override string Type => "Image";

        public ImageComponentViewModel(ImageComponent component, string userAccount, double ratio = 1) : base(component, userAccount, ratio)
        {
            effectDuration = component.EffectDuration;
            componentEffectKey = component.ComponentEffect;
            componentEffect = Effects.FirstOrDefault(c => c.Key == component.ComponentEffect)!.Name;
        }

        public override ImageComponent ToModel(string userAccount, double ratio)
        {
            return new ImageComponent()
            {
                Id = Id,
                Name = Name,
                ZIndex = ZIndex,
                Type = (MediaType)Enum.Parse(typeof(MediaType), Type),
                Left = Left / ratio,
                Top = Top / ratio,
                Width = Width / ratio,
                Height = Height / ratio,
                Source = Source == null ? string.Empty : Source.Replace(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, userAccount) + "\\", string.Empty),
                Timeline = Timeline,
                PlayCount = PlayCount,
                PlayDuration = PlayDuration,
                ComponentEffect = ComponentEffectKey,
                EffectDuration = EffectDuration,
            };
        }

        public static ImageComponentViewModel CreateInstance(string userAccount, int id)
        {
            return new ImageComponentViewModel(new ImageComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_104")}{id}",
                ZIndex = 1,
                Type = MediaType.Image,
                PlayCount = 1,
                PlayDuration = "00:00:05",
                Timeline = 5,
                ComponentEffect = "FadeIn",
                EffectDuration = 1000
            }, userAccount);
        }

        protected override FrameworkElement DrawingContent()
        {
            Image image = new()
            {
                Source = GetBitmap(Source!),
                Stretch = Stretch.Fill,
            };

            Border result = CreateBorder();
            result.Child = image;

            CreateBinding(result, FrameworkElement.WidthProperty, nameof(Width));
            CreateBinding(result, FrameworkElement.HeightProperty, nameof(Height));


            Canvas.SetLeft(result, Left);
            Canvas.SetTop(result, Top);
            Canvas.SetZIndex(result, ZIndex);

            // 添加鼠标事件处理
            result.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            result.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            result.MouseMove += Element_MouseMove;
            result.MouseWheel += Element_MouseWheel;

            FrameworkElement = result;

            return result;
        }

        protected override FrameworkElement DrawingRunningContent()
        {
            Image result = new()
            {
                Source = GetBitmap(Source!),
                Width = Width * Ratio,
                Height = Height * Ratio,
                Stretch = Stretch.Fill,
            };

            IsRunningLoaded = false;
            result.Loaded += (sender, e) =>
            {
                IsRunningLoaded = true;
                if (sender is Image image)
                {
                    RunningElement = image;
                    InitializeTimer(image);
                }
            };
            result.Unloaded += (sender, e) =>
            {
                if (sender is Image image)
                {
                    DisposeImage(image);
                    currentPlayCount = 0;
                    _timer = null;
                }
            };

            Canvas.SetLeft(result, Left * Ratio);
            Canvas.SetTop(result, Top * Ratio);
            Canvas.SetZIndex(result, ZIndex);
            return result;
        }

        public override void EffectExecution()
        {
            if (ComponentEffectKey != null)
            {
                Effects.Find(c => c.Key == ComponentEffectKey)?.Action(RunningElement);
            }
        }

        protected override void DisposeContent()
        {
            if (FrameworkElement is Image target)
            {
                DisposeImage(target);
            }
        }

        private BitmapImage GetBitmap(string source)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(source);
            bitmap.EndInit();

            return bitmap;
        }

        private void DisposeImage(Image target)
        {
            BitmapImage bitmap = (BitmapImage)target.Source;
            if (bitmap != null)
            {
                bitmap.UriSource = new Uri("about:blank");
                bitmap.DecodePixelHeight = 0;
                bitmap.DecodePixelWidth = 0;
                target.Source = null;
            }

            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        protected override void FadeIn(FrameworkElement element)
        {
            if (element is Image image)
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation
                {
                    From = 0.0, // 初始不透明度为0（完全透明）
                    To = 1.0,   // 最终不透明度为1（完全不透明）
                    Duration = new Duration(TimeSpan.FromMilliseconds(EffectDuration)) // 持续时间为2秒
                };

                // 将动画应用到Image的Opacity属性
                Storyboard.SetTarget(doubleAnimation, image);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Image.OpacityProperty));

                // 将动画添加到Storyboard中
                storyboard.Children.Add(doubleAnimation);

                // 开始Storyboard
                storyboard.Begin();
            }
            
        }

        private void Blinds(Image image)
        {
            var storyboard = new Storyboard();
            int windowHeight = (int)image.ActualHeight;
            int sliceHeight = windowHeight / 10;

            var clipGroup = new GeometryGroup();
            for (int i = 0; i < 10; i++)
            {
                // 创建一个矩形来作为每一叶百叶窗
                var rectangle = new RectangleGeometry
                {
                    Rect = new Rect (i * sliceHeight, 0, image.ActualWidth, sliceHeight)
                };

                // 将矩形添加到图像的剪裁区域
                clipGroup.Children.Add(rectangle);


                // 创建一个双关键帧动画来改变矩形的不透明度
                var doubleAnimation = new DoubleAnimation
                {
                    From = image.ActualWidth,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(EffectDuration)),
                    BeginTime = TimeSpan.FromMilliseconds(i * 100) // 每一叶百叶窗依次开始动画
                };

                // 将动画应用到矩形的不透明度属性
                Storyboard.SetTarget(doubleAnimation, rectangle);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(RectangleGeometry.TransformProperty));


                image.Clip = clipGroup;

                rectangle.SetValue(RectangleGeometry.RectProperty, new Rect(rectangle.Rect.X, rectangle.Rect.Y, image.ActualHeight, rectangle.Rect.Height));

                // 将动画添加到Storyboard中
                storyboard.Children.Add(doubleAnimation);
            }


            // 开始Storyboard
            storyboard.Begin();
        }

        private void InitializeTimer(Image target)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(Timeline);
            _timer.Tick += (s, e) => Timer_Tick(target);
            _timer.Start();
            currentPlayCount++;
        }

        private void Timer_Tick(Image target)
        {
            if (currentPlayCount < PlayCount)
            {
                if (ComponentEffect != null)
                {
                    Effects.Find(c => c.Key == ComponentEffectKey)?.Action(target);
                }
                currentPlayCount++;
            }
            else
            {
                var canvas = FindCanvasParent(target);
                if (canvas != null) 
                {
                    var existControl = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(c => c is Image image && image.Source == target.Source);
                    if (existControl != null) 
                    {
                        canvas.Children.Remove(existControl);
                    }
                }
            }
        }
    }
}
