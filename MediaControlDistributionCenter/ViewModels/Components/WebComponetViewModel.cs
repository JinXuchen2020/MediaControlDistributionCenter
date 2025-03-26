using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    public partial class WebComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Web";

        private BitmapSource thumbnail;

        private DispatcherTimer? _timer;
        private int currentPlayCount = 0;

        public WebComponentViewModel(WebComponent component, double ratio = 1) : base(component, ratio)
        {
        }

        public static WebComponentViewModel CreateInstance(int id)
        {
            return new WebComponentViewModel(new WebComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_106")}{id}",
                ZIndex = 1,
                Type = MediaType.Web,
                Timeline = 5,
                PlayCount = 1,
                PlayDuration = "00:00:05",
                Source = "https://www.baidu.com"
            });
        }

        public override WebComponent ToModel(double ratio)
        {
            return new WebComponent
            {
                Id = Id,
                Name = Name,
                ZIndex = ZIndex,
                Type = (MediaType)Enum.Parse(typeof(MediaType), Type),
                Left = Left / ratio,
                Top = Top / ratio,
                Width = Width / ratio,
                Height = Height / ratio,
                Source = Source,
                Timeline = Timeline,
                PlayCount = PlayCount,
                PlayDuration = PlayDuration
            };
        }

        protected override FrameworkElement DrawingContent()
        {
            Image result = new()
            {
                Source = GetBitmap(),
                //Width = Width,
                //Height = Height,
                //DataContext = this,
                
            };

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
            WebBrowser result = new WebBrowser
            {
                Source = string.IsNullOrEmpty(Source) ? null : new Uri(Source),
                Width = Width * Ratio,
                Height = Height * Ratio,
            };

            IsRunningLoaded = false;
            result.Loaded += (sender, e) =>
            {
                if (sender is WebBrowser target)
                {
                    target.Navigate(Source);
                    InitializeTimer(target);
                    IsRunningLoaded = true;
                }
            };
            result.Unloaded += (sender, e) =>
            {
                DisposeContent();
                currentPlayCount = 0;
                _timer = null;
            };

            Canvas.SetLeft(result, Left * Ratio);
            Canvas.SetTop(result, Top * Ratio);
            Canvas.SetZIndex(result, ZIndex);
            return result;
        }

        protected override void DisposeContent()
        {
            if (FrameworkElement is WebBrowser target)
            {
                target.Dispose();
            }
        }
        private void InitializeTimer(WebBrowser target)
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

        private void Timer_Tick(WebBrowser target)
        {
            if (currentPlayCount < PlayCount)
            {
                currentPlayCount++;
            }
            else
            {
                var canvas = FindCanvasParent(target);
                if (canvas != null)
                {
                    var existControl = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(c => c is WebBrowser browser && browser.Source == target.Source);
                    if (existControl != null)
                    {
                        canvas.Children.Remove(existControl);
                    }
                }
            }
        }
    }
}
