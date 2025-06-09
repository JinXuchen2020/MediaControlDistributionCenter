using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Web.WebView2.Wpf;
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

        private DispatcherTimer? _timer;
        private int currentPlayCount = 0;

        public WebComponentViewModel(WebComponent component, string userAccount, double ratio = 1) : base(component, userAccount, ratio)
        {
        }

        public static WebComponentViewModel CreateInstance(string userAccount, int id)
        {
            return new WebComponentViewModel(new WebComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_106")}{id}",
                ZIndex = id,
                Type = MediaType.Web,
                Timeline = 5,
                PlayCount = 1,
                PlayDuration = "00:00:05",
                Source = "https://www.baidu.com"
            }, userAccount);
        }

        public override WebComponent ToModel(string userAccount, double ratio)
        {
            return new WebComponent
            {
                Id = Id,
                Name = Name,
                ZIndex = ZIndex,
                Type = (MediaType)Enum.Parse(typeof(MediaType), Type),
                Left = Left,
                Top = Top,
                Width = Width,
                Height = Height,
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
                Stretch = Stretch.Fill,                
            };

            Border border = CreateBorder(result);

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
            WebView2 result = new WebView2() 
            {
                Source = string.IsNullOrEmpty(Source) ? null : new Uri(Source),
                Width = Width * Ratio,
                Height = Height * Ratio,
                IsHitTestVisible = false
            };

            IsRunningLoaded = false;
            result.Loaded += async (sender, e) =>
            {
                if (sender is WebView2 target)
                {
                    await target.EnsureCoreWebView2Async();
                    target.CoreWebView2.Navigate(Source);
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
            if (FrameworkElement is WebView2 target)
            {
                target.Dispose();
            }
        }
        private void InitializeTimer(WebView2 target)
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

        private void Timer_Tick(WebView2 target)
        {
            if (currentPlayCount < PlayCount)
            {
                target.CoreWebView2.Reload();
            }
            else
            {
                currentPlayCount = 0;
                var canvas = FindCanvasParent(target);
                if (canvas != null)
                {
                    var existControl = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(c => c is WebView2 browser && browser.Source == target.Source);
                    if (existControl != null)
                    {
                        canvas.Children.Remove(existControl);
                    }
                }
            }
        }
    }
}
