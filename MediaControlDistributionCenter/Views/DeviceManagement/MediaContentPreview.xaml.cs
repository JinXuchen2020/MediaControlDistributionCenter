using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services.LocalImps;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.Diagrams;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using MediaControlDistributionCenter.Services.ApiImps;

namespace MediaControlDistributionCenter.Views.DeviceManagement
{
    /// <summary>
    /// MediaContentPreview.xaml 的交互逻辑
    /// </summary>
    public partial class MediaContentPreview : Window
    {
        private readonly MediaViewModel viewModel;
        public MediaContentPreview(MediaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.viewModel = viewModel;

            this.Loaded += MediaContentPreview_Loaded;
        }

        private void MediaContentPreview_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(async () =>
            {
                if (viewModel.Type == "Video")
                {
                    var element = await DrawingVideo(viewModel);
                    MainCanvas.Children.Add(element);
                }
                else if (viewModel.Type == "Image")
                {
                    var element = await DrawingImage(viewModel);
                    MainCanvas.Children.Add(element);
                }
            });
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected async Task<FrameworkElement> DrawingImage(MediaViewModel viewModel)
        {
            var source = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, viewModel.Src);
            if (!File.Exists(source))
            {
                var uploadService = Utility.GetService<IUploadService>();
                if (uploadService is UploadServiceLocal local)
                {
                    var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                    local.FtpClient = ftpClient;
                }

                await uploadService.DownloadFile(viewModel.Src);
            }
            Image result = new()
            {
                Source = GetBitmap(source),
                Width = viewModel.Width > 0 && viewModel.Width < MainCanvas.Width ? viewModel.Width : MainCanvas.Width,
                Height = viewModel.Height > 0 && viewModel.Height < MainCanvas.Height ? viewModel.Height : MainCanvas.Height,
                Stretch = Stretch.Fill
            };

            result.Unloaded += (sender, e) =>
            {
                if (sender is Image image)
                {
                    DisposeImage(image);
                }
            };

            Canvas.SetLeft(result, 0);
            Canvas.SetTop(result, 0);
            Canvas.SetZIndex(result, 1);
            return result;
        }

        protected async Task<FrameworkElement> DrawingVideo(MediaViewModel viewModel)
        {
            var source = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, viewModel.Src);
            if (!File.Exists(source))
            {
                var uploadService = Utility.GetService<IUploadService>();
                if (uploadService is UploadServiceLocal local)
                {
                    var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                    local.FtpClient = ftpClient;
                }

                await uploadService.DownloadFile(viewModel.Src);
            }
            MediaElement result = new()
            {
                Source = new Uri(source),
                Width = viewModel.Width > 0 && viewModel.Width < MainCanvas.Width ? viewModel.Width : MainCanvas.Width,
                Height = viewModel.Height > 0 && viewModel.Height < MainCanvas.Height ? viewModel.Height : MainCanvas.Height,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
                Stretch = Stretch.Fill
            };

            Canvas.SetLeft(result, 0);
            Canvas.SetTop(result, 0);
            Canvas.SetZIndex(result, 1);

            // 添加鼠标事件处理
            result.MediaFailed += (sender, e) =>
            {
                MessageBox.Show($"视频加载失败，错误: {e.ErrorException.Message}");
            };

            result.MediaEnded += (sender, e) =>
            {
                result.Position = TimeSpan.Zero;
                result.Play();
            };

            result.Unloaded += (sender, e) =>
            {
                result.Stop();
                result.Source = null;
            };

            result.Play();
            return result;
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

        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
