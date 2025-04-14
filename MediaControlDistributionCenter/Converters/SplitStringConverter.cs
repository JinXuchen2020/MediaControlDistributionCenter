using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaControlDistributionCenter.Converters
{
    public class SplitStringConverter : IValueConverter
    {
        private Dictionary<int, string> splitString = new Dictionary<int, string>();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var index = parameter == null ? 0 : int.Parse(parameter.ToString()!);
            if (value is string str)
            {
                var splitList = str.Split("-");

                if (splitList.Length == 1 && index > 0)
                {
                    return splitList[0];
                }

                if (splitString.ContainsKey(index))
                {
                    splitString[index] = str.Split("-")[index];
                }
                else
                {
                    splitString.Add(index, str);
                }
                return str.Split("-")[index];
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var index = parameter == null ? 0 : int.Parse(parameter.ToString()!);
            if (value is DateTime str)
            {
                splitString[index] = str.TimeOfDay.ToString();
            }

            if (value is string strValue)
            {
                splitString[index] = strValue;
            }
            return string.Join("-", splitString.Select(c => c.Value));
        }
    }
}
