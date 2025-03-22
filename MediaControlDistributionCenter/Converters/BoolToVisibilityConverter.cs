using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                var expectedValue = true;
                if (parameter != null)
                {
                    expectedValue = bool.Parse(parameter.ToString()!);
                }

                return isVisible == expectedValue ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (parameter != null)
            {
                var expectedValues = parameter.ToString()!.Split(";").ToList();
                if (expectedValues.Contains(value.ToString())) 
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
