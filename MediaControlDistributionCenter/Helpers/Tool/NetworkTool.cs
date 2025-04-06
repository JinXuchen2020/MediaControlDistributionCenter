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
            List<string> addresses = new();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 过滤掉虚拟网络接口和不启用的接口
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipPro = ni.GetIPProperties();
                    if (!ipPro.IsDynamicDnsEnabled)
                    {
                        foreach (var ip in ipPro.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                addresses.Add(ip.Address.ToString());
                            }
                        }
                    }
                }
            }
            return addresses;
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
