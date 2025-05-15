using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class ConnectService : Proxy, IConnectService
    {
        public ConnectService(ConnectionMode options) : base(options)
        {
        }

        public async Task<bool> ExecuteCmdAsync(string Cmd, TimeSpan waitExecTime)
        {
            // 设置
            if (!Cmd.EndsWith("##End##"))
            {
                Cmd = string.Concat(Cmd, "##End##");
            }

            var url = "/monitor/sendCmd";
            var result = await Post<ResultResponse<bool>, Command>(url, new Command { Cmd = Cmd });
            if (result == null)
            {
                return false;
            }

            return result.Data;
        }
    }

    public class Command
    {
        [JsonProperty("cmd", NullValueHandling = NullValueHandling.Ignore)]
        public string Cmd { get; set; }
    }
}
