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
    public class MultiSelectedValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 1)
            {
                var dataValue = values[0].ToString()!;
                var dataParameter = values[1].ToString()!;

                if (dataValue.Contains('#'))
                {
                    if (dataValue.Contains(dataParameter))
                    {
                        
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30479C"));
                    }
                }

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1E23"));
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D1E23"));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
