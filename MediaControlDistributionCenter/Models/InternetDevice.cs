using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Models
{
    public class InternetDevice
    {
        public string SnCode { get; set; }

        public string IpAddress { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public string UserAccount { get; set; }

        public bool IsInternet { get; set; }

        public string TypeText { get; set; }

        public Communication? Communication { get; set; }

        public DeviceViewModel? DeviceViewModel { get; set; }
    }
}
