using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Views.CustomControls;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Syncfusion.Presentation;
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
        private double totalTimeline;

        private int currentPlayCount;
        private DispatcherTimer? _timer;

        public VideoComponentViewModel(VideoComponent component, string userAccount, double ratio = 1) : base(component, userAccount, ratio)
        {
            playMode = component.PlayMode;
        }

        public static VideoComponentViewModel CreateInstance(string userAccount, int id)
        {
            return new VideoComponentViewModel(new VideoComponent
            {
                Id = id,
                Name = $"{FindResource("LanguageKey_Code_ProgramEdit_Tooltip_103")}{id}",
                ZIndex = id,
                PlayMode = "fullscreen",
                Type = MediaType.Video,
                PlayCount = 1,
                Timeline = 0,
                PlayDuration = "00:00:00",
            }, userAccount);
        }

        public override VideoComponent ToModel(string userAccount, double ratio)
        {
            return new VideoComponent
            {
                Id = Id,
                Name = Name,
                ZIndex = ZIndex,
                Type = (MediaType)Enum.Parse(typeof(MediaType), Type),
                Left = Left,
                Top = Top,
                Width = Width,
                Height = Height,
                Source = Source == null ? string.Empty : Source.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, userAccount) + "\\", string.Empty),
                Timeline = Timeline,
                PlayMode = PlayMode,
                PlayCount = PlayCount,
                PlayDuration = PlayDuration,
                IsClip = Timeline != TotalTimeline
            };
        }

        protected override FrameworkElement DrawingContent()
        {
            MediaElement video = new()
            {
                Source = new Uri(Source!),
                Width = Width * Ratio,
                Height = Height * Ratio,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
            };

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
                Stretch = Stretch.Fill,
            };

            Canvas.SetLeft(result, Left * Ratio);
            Canvas.SetTop(result, Top * Ratio);
            Canvas.SetZIndex(result, ZIndex);

            IsRunningLoaded = false;
            result.MediaOpened += (sender, e) =>
            {
                IsRunningLoaded = true;

                if (sender is MediaElement mediaElement)
                {
                    InitializeTimer(mediaElement);
                }
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
                if (sender is MediaElement mediaElement)
                {
                    _timer?.Stop();
                    _timer = null;
                    currentPlayCount = 0;
                }
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
                _timer?.Stop();
                _timer = null;
                currentPlayCount = 0;
            }

            if (FrameworkElement is Border)
            {
                Timeline = 0;
                TotalTimeline = 0;
                thumbnail = null;
            }
        }

        private void Video_LayoutUpdated(object sender, EventArgs e)
        {
            if (FrameworkElement == null)
            {
                var video = (sender as MediaElement)!;
                if (Timeline == 0)
                {
                    Timeline = video.NaturalDuration.TimeSpan.TotalSeconds;
                    TotalTimeline = Timeline;
                    PlayDuration = TimeSpan.FromSeconds(Timeline).ToString();
                }
                else 
                {
                    TotalTimeline = video.NaturalDuration.TimeSpan.TotalSeconds;
                }

                video.Position = TimeSpan.FromSeconds(1);

                var canvas = FindCanvasParent(video);
                thumbnail = thumbnail ?? CaptureMediaElement(video);

                Image image = new()
                {
                    Source = thumbnail,
                    Stretch = Stretch.Fill,
                };

                Border result = CreateBorder(image);

                canvas.Children.Add(result);
                canvas.Children.Remove(video);
                video.Stop();
                video.Source = null;

                FrameworkElement = result;
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

            //return VideoScreenCapture.CaptureFrame(Source!, 1);

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

        private void InitializeTimer(MediaElement target)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(Timeline);
            _timer.Tick += (s, e) => Timer_Tick(target);
            _timer.Start();
        }

        private void Timer_Tick(MediaElement target)
        {
            if (currentPlayCount < PlayCount)
            {
                target.Position = TimeSpan.Zero;
                target.Play();
                //InitializeTimer(target);
                currentPlayCount++;
            }
            else
            {
                var canvas = FindCanvasParent(target);
                if (canvas != null)
                {
                    var existControl = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(c => c is MediaElement media && media.Source == target.Source);
                    if (existControl != null)
                    {
                        canvas.Children.Remove(existControl);
                    }
                }
            }
        }
    }
}
