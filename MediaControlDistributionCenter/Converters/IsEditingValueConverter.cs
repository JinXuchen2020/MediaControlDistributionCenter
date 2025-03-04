using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class IsEditingValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string expectedValue = parameter?.ToString() ?? string.Empty;
            var expectedValues = parameter!.ToString()!.Split(";").ToList();
            if (value != null)
            {
                if (expectedValues.Contains(value!.ToString()))
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
