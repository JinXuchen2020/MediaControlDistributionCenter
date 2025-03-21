using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class SelectedValueConverter : IValueConverter
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

                return isVisible == expectedValue ? "#30479C" : "#1D1E23";
            }
            else if (value != null && parameter != null)
            {
                var expectedValues = parameter.ToString()!.Split(";").ToList();
                if (expectedValues.Contains(value))
                {
                    return "#30479C";
                }
                return "#1D1E23";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
