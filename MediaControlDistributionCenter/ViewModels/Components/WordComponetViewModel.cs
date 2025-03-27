using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PdfiumViewer;
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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class WordComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Word";

        [ObservableProperty]
        private int effectDuration;  //特效时长    -毫秒

        [ObservableProperty]
        private string componentEffect; //"上下展开",

        [ObservableProperty]
        private string componentEffectKey;

        [ObservableProperty]
        private int pageDuration;

        private DispatcherTimer? _timer;
        private int currentPlayPage = 1;
        private FrameworkElement RunningElement;

        public WordComponentViewModel(WordComponent component, double ratio = 1) : base(component, ratio)
        {
            effectDuration = component.EffectDuration;
            componentEffectKey = component.ComponentEffect;
            componentEffect = Effects.FirstOrDefault(c => c.Key == component.ComponentEffect)!.Name;
            pageDuration = component.PageDuration;
        }

        public static WordComponentViewModel CreateInstance(int id)
        {
            return new WordComponentViewModel(new WordComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_107")}{id}",
                ZIndex = 1,
                Type = MediaType.Word,
                Timeline = 5,
                PlayCount = 1,
                PlayDuration = "00:00:05",
                ComponentEffect = "FadeIn",
                EffectDuration = 1000,
                PageDuration = 10,
            });
        }

        public override WordComponent ToModel(double ratio)
        {
            return new WordComponent
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
                PlayDuration = PlayDuration,
                ComponentEffect = ComponentEffectKey,
                EffectDuration = EffectDuration,
                PageDuration = PageDuration
            };
        }

        protected override FrameworkElement DrawingContent()
        {
            Image result = CapturePage(1);

            Border border = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = Width,
                Height = Height,
                DataContext = this,
                Child = result
            };

            CreateBinding(border, FrameworkElement.WidthProperty, nameof(Width));
            CreateBinding(border, FrameworkElement.HeightProperty, nameof(Height));

            Canvas.SetLeft(border, Left);
            Canvas.SetTop(border, Top);
            Canvas.SetZIndex(border, ZIndex);

            // 添加鼠标事件处理
            border.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            border.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            border.MouseMove += Element_MouseMove;
            border.MouseWheel += Element_MouseWheel;

            FrameworkElement = border;

            return border;
        }

        private BitmapImage GetBitmap()
        {
            var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/site-{Type.ToLower()}.png", UriKind.Absolute);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = uri;
            bitmap.EndInit();

            return bitmap;
        }

        protected override FrameworkElement DrawingRunningContent()
        {
            currentPlayPage = 1;
            var result = CapturePage(currentPlayPage);
            if (result != null)
            {
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
                    }
                };

                Canvas.SetLeft(result, Left * Ratio);
                Canvas.SetTop(result, Top * Ratio);
                Canvas.SetZIndex(result, ZIndex);
            }
            
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
            if (FrameworkElement is Image image)
            {
                DisposeImage(image);
                currentPlayPage = 1;
                _timer = null;
            }
        }

        private Image? CapturePage(int page)
        {
            if (!string.IsNullOrEmpty(Source))
            {
                var pdfDocument = PdfDocument.Load(Source);
                if (page <= pdfDocument.PageCount)
                {
                    var image = pdfDocument.Render(page - 1, 229, 329, true);
                    var bitmapImage = BitmapSourceFromImage(image);
                    var imageControl = new Image { Source = bitmapImage, Width = Width, Height = Height };
                    return imageControl;
                }
            }

            return null;
        }

        private BitmapSource BitmapSourceFromImage(System.Drawing.Image image)
        {
            var bitmap = new System.Drawing.Bitmap(image);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private void DisposeImage(Image target)
        {
            var bitmap = target.Source as BitmapImage;
            if (bitmap != null)
            {
                bitmap.UriSource = new Uri("about:blank");
                bitmap.DecodePixelHeight = 0;
                bitmap.DecodePixelWidth = 0;
                target.Source = null;
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

        private void InitializeTimer(Image target)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(PageDuration);
            _timer.Tick += (s, e) => Timer_Tick(target);
            _timer.Start();
        }

        private void Timer_Tick(Image target)
        {
            currentPlayPage++;
            var image = CapturePage(currentPlayPage);
            var canvas = FindCanvasParent(target);
            if (canvas != null)
            {
                var existControl = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(c => c is Image image && image.Source == target.Source);
                if (existControl != null)
                {
                    canvas.Children.Remove(existControl);
                }

                if (image != null)
                {
                    canvas.Children.Add(image);
                    RunningElement = image;
                    EffectExecution();
                }
            }
        }
    }
}
