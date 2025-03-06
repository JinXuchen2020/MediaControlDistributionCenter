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
    public partial class VideoComponentViewModel : BaseComponentViewModel
    {
        public override string Type => "Video";

        private BitmapSource thumbnail;

        [ObservableProperty]
        private string playMode;

        [ObservableProperty]
        private int playCount;

        [ObservableProperty]
        private string playDuration;

        private int currentPlayCount;

        public VideoComponentViewModel(VideoComponent component, double ratio = 1) : base(component, ratio)
        {
            playMode = component.PlayMode;
            playCount = component.PlayCount;
            playDuration = component.PlayDuration;
        }

        public override VideoComponent ToModel(double ratio)
        {
            return new VideoComponent
            {
                Id = Id,
                Name = Name,
                ZIndex = ZIndex,
                Type = (MediaType)Enum.Parse(typeof(MediaType), Type),
                Left = Left / ratio,
                Top = Top / ratio,
                Width = Width / ratio,
                Height = Height / ratio,
                Source = Source == null ? string.Empty : Source.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath) + "\\", string.Empty),
                Timeline = Timeline,
                PlayMode = PlayMode,
                PlayCount = PlayCount,
                PlayDuration = PlayDuration
            };
        }

        protected override FrameworkElement DrawingContent()
        {
            MediaElement video = new()
            {
                Source = new Uri(Source!),
                Width = Width,
                Height = Height,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
            };

            video.MediaOpened += Video_MediaOpened;
            video.SizeChanged += Video_LayoutUpdated;
            video.Play();

            return video;
        }

        protected override FrameworkElement DrawingRunningContent()
        {
            MediaElement result = new()
            {
                Source = new Uri(Source!),
                Width = Width * Ratio,
                Height = Height * Ratio,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
            };

            Canvas.SetLeft(result, Left * Ratio);
            Canvas.SetTop(result, Top * Ratio);
            Canvas.SetZIndex(result, ZIndex);

            // 添加鼠标事件处理
            result.MediaFailed += (sender, e) =>
            {
                MessageBox.Show($"视频加载失败，错误: {e.ErrorException.Message}");
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

        private void Video_LayoutUpdated(object sender, EventArgs e)
        {
            if (FrameworkElement == null)
            {
                var video = (sender as MediaElement)!;

                video.Position = TimeSpan.FromSeconds(1);

                var canvas = FindCanvasParent(video);
                thumbnail = thumbnail ?? CaptureMediaElement(video);

                Image result = new()
                {
                    Source = thumbnail,
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

                canvas.Children.Add(result);
                canvas.Children.Remove(video);
                video.Stop();
                video.Source = null;

                FrameworkElement = result;
            }
        }

        private void Video_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (FrameworkElement == null)
            {
                var video = (sender as MediaElement)!;

                if (video.NaturalDuration.HasTimeSpan)
                {
                    Timeline = video.NaturalDuration.TimeSpan.TotalSeconds;
                    PlayDuration = video.NaturalDuration.TimeSpan.ToString();
                }
            }
            
        }

        private BitmapSource CaptureMediaElement(MediaElement mediaElement)
        {
            // 确保MediaElement已经加载了媒体

            System.Threading.Thread.Sleep(100);
            if (mediaElement.Source == null)
            {
                throw new InvalidOperationException("MediaElement没有加载媒体源。");
            }

            return mediaElement.Dispatcher.Invoke<BitmapSource>(() =>
            {
                // 创建一个RenderTargetBitmap
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                    (int)mediaElement.ActualWidth,
                    (int)mediaElement.ActualHeight,
                    96, 96,
                    PixelFormats.Pbgra32);
                mediaElement.Measure(new Size(mediaElement.ActualWidth, mediaElement.ActualHeight));
                mediaElement.Arrange(new Rect(new Size(mediaElement.ActualWidth, mediaElement.ActualHeight)));

                // 将MediaElement绘制到RenderTargetBitmap
                renderTargetBitmap.Render(mediaElement);
                return renderTargetBitmap;

                //// 保存位图为PNG文件
                //PngBitmapEncoder png = new PngBitmapEncoder();
                //png.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                //using (Stream stream = File.Create("screenshot.png"))
                //{
                //    png.Save(stream);
                //}
            });
        }
    }
}
