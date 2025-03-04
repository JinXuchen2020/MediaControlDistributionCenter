using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class StringToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && TimeSpan.TryParse(str, out TimeSpan result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan span)
            {
                return span.ToString();
            }
            return "00:00:00";
        }
    }
}
