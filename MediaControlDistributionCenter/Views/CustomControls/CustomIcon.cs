using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                if (e.NewValue != null) 
                {
                    var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/svg-{e.NewValue.ToString()!.ToLower()}.png", UriKind.Absolute);
                    var resourceStream = Application.GetResourceStream(uri);

                    if (resourceStream != null)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = resourceStream.Stream;
                        bitmap.EndInit();

                        d.SetCurrentValue(SourceProperty, bitmap);
                    }
                }
            }));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapSource), typeof(CustomIcon), new PropertyMetadata(null));

        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
    }
}
