using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.FTP
{
    public class FtpConnection
    {
        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string UserPassword { get; set; }

        public string BasePath { get; set; }
    }
}
