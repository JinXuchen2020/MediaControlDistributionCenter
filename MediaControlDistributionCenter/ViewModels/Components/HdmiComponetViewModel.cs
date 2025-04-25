using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
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
    public partial class HdmiComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Hdmi";

        private int currentPlayCount = 0;

        public HdmiComponentViewModel(HdmiComponent component, string userAccount, double ratio = 1) : base(component, userAccount, ratio)
        {
        }

        public static HdmiComponentViewModel CreateInstance(string userAccount, int id)
        {
            return new HdmiComponentViewModel(new HdmiComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_111")}{id}",
                ZIndex = id,
                Type = MediaType.Hdmi,
                Timeline = 5,
                PlayCount = 1,
                PlayDuration = "00:00:05",
                Source = "https://www.baidu.com"
            }, userAccount);
        }

        public override HdmiComponent ToModel(string userAccount, double ratio)
        {
            return new HdmiComponent
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
            var source = CaptureHdmiStream();
            MediaElement result = new()
            {
                Source = new Uri(source),
                Width = Width * Ratio,
                Height = Height * Ratio,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
            };

            Canvas.SetLeft(result, Left * Ratio);
            Canvas.SetTop(result, Top * Ratio);
            Canvas.SetZIndex(result, ZIndex);

            IsRunningLoaded = false;
            result.MediaOpened += (sender, e) =>
            {
                IsRunningLoaded = true;
            };

            // 添加鼠标事件处理
            result.MediaFailed += (sender, e) =>
            {
                MessageBox.Show($"视频加载失败，错误: {e.ErrorException.Message}");
                IsRunningLoaded = true;
            };

            result.MediaEnded += (sender, e) =>
            {
                if (currentPlayCount < PlayCount)
                {
                    result.Position = TimeSpan.Zero;
                    result.Play();
                    currentPlayCount++;
                }
            };

            result.Unloaded += (sender, e) =>
            {
                result.Stop();
                result.Source = null;
            };

            result.Play();
            currentPlayCount = 1;
            return result;
        }

        protected override void DisposeContent()
        {
            if (FrameworkElement is MediaElement target)
            {
                target.Stop();
                target.Source = null;
            }
        }

        private string CaptureHdmiStream()
        {
            return string.Empty;
        }
    }
}
