using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.Views.CustomControls
{
    public class CustomIcon : Control
    {
        static CustomIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomIcon), new FrameworkPropertyMetadata(typeof(CustomIcon)));
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(CustomIcon), new PropertyMetadata(null, (d, e) =>
            {
                if (d is CustomIcon icon && e.NewValue != null) 
                {
                    var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/svg-{e.NewValue.ToString()!.ToLower()}.png", UriKind.Absolute);
                    var resourceStream = Application.GetResourceStream(uri);

                    if (resourceStream != null)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = resourceStream.Stream;
                        bitmap.EndInit();

                        if(icon.IsSelected)
                        {
                            d.SetCurrentValue(SourceProperty, GetInvertedImage(bitmap));
                        }
                        else
                        {
                            d.SetCurrentValue(SourceProperty, bitmap);
                        }
                    }
                }
            }));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapSource), typeof(CustomIcon), new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(CustomIcon), new PropertyMetadata(false, (d, e) =>
            {
                if (d is CustomIcon icon && e.NewValue != null) 
                {
                    var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/svg-{icon.Type.ToLower()}.png", UriKind.Absolute);
                    var resourceStream = Application.GetResourceStream(uri);

                    if (resourceStream != null)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = resourceStream.Stream;
                        bitmap.EndInit();

                        if(e.NewValue is bool selected && selected)
                        {
                            d.SetCurrentValue(SourceProperty, GetInvertedImage(bitmap));
                        }
                        else
                        {
                            d.SetCurrentValue(SourceProperty, bitmap);
                        }
                    }
                }
            }));

        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // 在代码中动态生成反色图像
        public static BitmapSource GetInvertedImage(BitmapSource source)
        {
            FormatConvertedBitmap converted = new FormatConvertedBitmap();
            converted.BeginInit();
            converted.Source = source;
            converted.DestinationFormat = PixelFormats.Bgra32; // 确保支持透明度
            converted.EndInit();

            // 像素处理（反色）
            int stride = converted.PixelWidth * 4;
            byte[] pixels = new byte[converted.PixelHeight * stride];
            converted.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]);     // B
                pixels[i + 1] = (byte)(255 - pixels[i + 1]); // G
                pixels[i + 2] = (byte)(255 - pixels[i + 2]); // R
            }

            return BitmapSource.Create(
                converted.PixelWidth, converted.PixelHeight,
                converted.DpiX, converted.DpiY,
                PixelFormats.Bgra32, null, pixels, stride
            );
        }
    }
}
