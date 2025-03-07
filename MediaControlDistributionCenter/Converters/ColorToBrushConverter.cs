using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaControlDistributionCenter.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color && targetType == typeof(Brush))
            {
                return new SolidColorBrush(color);
            }
            return Brushes.Black; // 默认颜色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush color && targetType == typeof(Color))
            {
                return ColorConverter.ConvertFromString(color.ToString());
            }

            return Colors.Black;
        }
    }
}
