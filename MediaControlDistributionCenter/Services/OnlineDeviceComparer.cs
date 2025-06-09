using MediaControlDistributionCenter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public class OnlineDeviceComparer : IEqualityComparer<InternetDevice>
    {
        public bool Equals(InternetDevice? x, InternetDevice? y)
        {
            if (x.SnCode == y.SnCode && x.Status == y.Status)
            {
                if(x.DeviceViewModel == null && y.DeviceViewModel == null)
                {
                    return true;
                }
                else if (x.DeviceViewModel != null && y.DeviceViewModel != null)
                {
                    var xModel = x.DeviceViewModel.ToModel();
                    var yModel = y.DeviceViewModel.ToModel();
                    var properties = xModel.GetType().GetProperties();
                    var result = true;
                    foreach(var property in properties)
                    {
                        if(property.Name != "Brightness" && property.Name != "Volume" && property.Name != "CurrentDataTime")
                        {
                            if (property.GetValue(xModel)?.ToString() != property.GetValue(yModel)?.ToString())
                            {
                                result = false;
                                break;
                            }
                        }
                    }

                    return result;
                }
            }

            return false;
        }

        public int GetHashCode([DisallowNull] InternetDevice obj)
        {
            return 1;
        }
    }
}
