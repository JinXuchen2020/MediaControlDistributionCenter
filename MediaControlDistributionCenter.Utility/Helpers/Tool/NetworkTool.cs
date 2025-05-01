using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            addresses.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return addresses;
        }

        public static List<string> GetLocalIPv4Address(string gatewayAddress)
        {
            List<string> addresses = new();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 过滤掉虚拟网络接口和不启用的接口
                if (ni.GetIPProperties().GatewayAddresses.Any(c=>c.Address.ToString() == gatewayAddress) && ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            addresses.Add(ip.Address.ToString());
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
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
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

        public static List<IPAddress> GetBroadcastIp()
        {
            var result = new List<IPAddress>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.Supports(NetworkInterfaceComponent.IPv4))
                {
                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            var ipBytes = ip.Address.GetAddressBytes();
                            var maskBytes = ip.IPv4Mask.GetAddressBytes();
                            var broadcastBytes = new byte[ipBytes.Length];
                            for(int i = 0; i < ipBytes.Length; i++)
                            {
                                broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
                            }

                            result.Add(new IPAddress(broadcastBytes));
                        }
                    }
                }
            }
            return result;
        }
    }
}
