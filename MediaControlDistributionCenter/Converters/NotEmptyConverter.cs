using System.Globalization;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class NotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;  // ConvertBack 可以根据需求实现
        }
    }

}
