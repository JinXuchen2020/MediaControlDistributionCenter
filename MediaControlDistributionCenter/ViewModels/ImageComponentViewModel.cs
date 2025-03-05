using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
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

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class ImageComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Image";

        [ObservableProperty]
        private int playCount;

        [ObservableProperty]
        private TimeSpan playDuration;

        [ObservableProperty]
        private int effectDuration;  //特效时长    -毫秒

        [ObservableProperty]
        public string componentEffect; //"上下展开",

        public Dictionary<string, Action<Image>> Effects { get; set; }

        public ImageComponentViewModel(ImageComponent component, double ratio = 1) : base(component, ratio)
        {
            playCount = component.PlayCount;
            playDuration = string.IsNullOrEmpty(component.PlayDuration) ? TimeSpan.Zero : TimeSpan.Parse(component.PlayDuration);
            effectDuration = component.EffectDuration;
            componentEffect = component.ComponentEffect;
            Effects = new Dictionary<string, Action<Image>>
            {
                { "向右扩展", FadeIn},
                { "向下扩展", FadeIn},
                { "向左扩展", FadeIn},
                { "向上扩展", FadeIn},
                { "中间向外扩展", FadeIn},
                { "左右扩展", FadeIn},
                { "上下扩展", FadeIn},
                { "向右平移", FadeIn},
                { "向下平移", FadeIn},
                { "向左平移", FadeIn},
                { "向上平移", FadeIn},
                { "向右压缩", FadeIn},
                { "向下压缩", FadeIn},
                { "向左压缩", FadeIn},
                { "向上压缩", FadeIn},
                { "上下压缩", FadeIn},
                { "左右压缩", FadeIn},
                { "向下展开卷轴", FadeIn},
                { "向上展开卷轴", FadeIn},
                { "水平百叶窗", Blinds},
                { "垂直百叶窗", FadeIn},
                { "变焦全屏", FadeIn},
                { "轮子", FadeIn},
                { "上下齿合", FadeIn},
                { "淡入", FadeIn},
                { "向右堆积", FadeIn},
                { "向下堆积", FadeIn},
                { "向左堆积", FadeIn},
                { "向上堆积", FadeIn},
                { "左镭射", FadeIn},
                { "上镭射", FadeIn},
                { "右镭射", FadeIn},
                { "下镭射", FadeIn},
                { "向下展开", FadeIn},
                { "向上展开", FadeIn},
                { "上下展开", FadeIn},
                { "上下合并", FadeIn},
            };
        }

        public override ImageComponent ToModel(double ratio)
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
                Source = Source == null ? string.Empty : Source.Replace(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath) + "\\", string.Empty),
                Timeline = Timeline,
                PlayCount = PlayCount,
                PlayDuration = TimeSpan.FromSeconds(Timeline).ToString(),
                ComponentEffect = ComponentEffect,
                EffectDuration = EffectDuration,
            };
        }

        protected override FrameworkElement DrawingContent()
        {
            Image result = new()
            {
                Source = GetBitmap(Source!),
                Width = Width,
                Height = Height,
                DataContext = this
            };

            var widthBinding = new Binding("Width")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };

            var heightBinding = new Binding("Height")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };

            result.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            result.SetBinding(FrameworkElement.HeightProperty, heightBinding);

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
            };

            result.Loaded += (sender, e) =>
            {
                if (sender is Image image && ComponentEffect != null)
                {
                    Effects[ComponentEffect](image);
                }
            };
            result.Unloaded += (sender, e) =>
            {
                DisposeImage(result);
            };

            Canvas.SetLeft(result, Left * Ratio);
            Canvas.SetTop(result, Top * Ratio);
            Canvas.SetZIndex(result, ZIndex);
            return result;
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
        }

        private void FadeIn(Image image)
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
    }
}
