using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IConnectService
    {
        public Task<bool> ExecuteCmdAsync(string Cmd, string deviceSnCode, TimeSpan waitExecTime);
    }
}
