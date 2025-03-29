using Microsoft.IdentityModel.Tokens;
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
    public class IsEditingValueConverter : IValueConverter
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

                return isVisible == expectedValue;
            }
            else if (parameter != null && value != null)
            {
                var expectedValues = parameter.ToString()!.Split(";").ToList();
                if (expectedValues.Contains(value.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
