using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.ViewModels;
using MediaControlDistributionCenter.Views.Diagrams;
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

namespace MediaControlDistributionCenter.Views.DeviceManagement
{
    /// <summary>
    /// MediaContentPreview.xaml 的交互逻辑
    /// </summary>
    public partial class MediaContentPreview : Window
    {
        public MediaContentPreview(MediaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            if (viewModel.Type == "Video")
            {
                var element = DrawingVideo(viewModel);
                MainCanvas.Children.Add(element);
            }
            else if (viewModel.Type == "Image")
            {
                var element = DrawingImage(viewModel);
                MainCanvas.Children.Add(element);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected FrameworkElement DrawingImage(MediaViewModel viewModel)
        {
            var source = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, viewModel.Src);
            Image result = new()
            {
                Source = GetBitmap(source),
                Width = Math.Min(viewModel.Width, MainCanvas.Width),
                Height = Math.Min(viewModel.Height, MainCanvas.Height),
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

        protected FrameworkElement DrawingVideo(MediaViewModel viewModel)
        {
            var source = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, viewModel.Src);
            MediaElement result = new()
            {
                Source = new Uri(source),
                Width = Math.Min(viewModel.Width, MainCanvas.Width),
                Height = Math.Min(viewModel.Height, MainCanvas.Height),
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
