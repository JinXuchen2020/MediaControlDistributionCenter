using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class ToMultipleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ratio = parameter == null ? 1 : double.Parse(parameter.ToString()!);
            if (value is double str)
            {
                return str * ratio;
            }

            if (value is string strValue && double.TryParse(strValue, out double doubleValue))
            {
                return doubleValue * ratio;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ratio = parameter == null ? 1 : double.Parse(parameter.ToString()!);
            if (value is double str)
            {
                return str / ratio;
            }

            if (value is string strValue && double.TryParse(strValue, out double doubleValue))
            {
                return doubleValue / ratio;
            }
            return 0;
        }
    }
}
