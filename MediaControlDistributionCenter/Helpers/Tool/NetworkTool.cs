using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Tool
{
    public static class NetworkTool
    {

        public static List<string> GetLocalIPv4Address()
        {
            List<string> addrs = new List<string>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 过滤掉虚拟网络接口和不启用的接口
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            addrs.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return addrs;
        }

        public static List<string> GetGatewayIp()
        {
            var result = new List<string>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var gatewayAddress = networkInterface.GetIPProperties().GatewayAddresses;
                    if (gatewayAddress != null)
                    {
                        foreach (var address in gatewayAddress)
                        {
                            result.Add(address.Address.ToString());                            
                        }
                    }
                }
            }
            return result;
        }
    }
}
