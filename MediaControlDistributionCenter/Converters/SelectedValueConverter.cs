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
                if (value is string strValue)
                {
                    if (strValue.Contains('#'))
                    {
                        var expectedValues = parameter.ToString()!;
                        if (strValue.Contains(expectedValues))
                        {
                            return "#30479C";
                        }
                    }
                    else
                    {
                        var expectedValues = parameter.ToString()!.Split(";").ToList();
                        if (expectedValues.Contains(strValue))
                        {
                            return "#30479C";
                        }
                    }
                }

                if (value is Enum enumValue)
                {
                    var expectedValues = parameter.ToString()!.Split(";").ToList();
                    if (expectedValues.Contains(enumValue.ToString()))
                    {
                        return "#30479C";
                    }
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
