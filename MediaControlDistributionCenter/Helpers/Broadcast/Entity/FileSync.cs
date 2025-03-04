using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Broadcast.Entity
{
    public class FileSync
    {
        public string HostName { get; set; }

        public int ServerPort { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public long FileSize { get; set; }

        public string FileName { get; set; }
    }
}
