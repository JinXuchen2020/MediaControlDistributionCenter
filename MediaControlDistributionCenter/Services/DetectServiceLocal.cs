using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Models;
using Serilog;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.FTP;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using MediaControlDistributionCenter.Services.DTO;
using System.Windows;

namespace MediaControlDistributionCenter.Services
{

    public class DetectServiceLocal : IDetectService
    {
        private readonly List<InternetDevice> onlineDevices = [];

        private readonly List<FtpServer> ftpServers = [];

        private const int BroadcastPort = 5001;//9876; // 广播端口
        private const int ListenPort = 5001;//9877;    // 接收回复端口
        private UdpClient _listener;

        public event EventHandler? InvokeDevicesChanged;

        public bool IsStarted => _listener != null;

        public async Task StartDetect()
        {
            var localDevice = onlineDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet);
            onlineDevices.Clear();
            if (localDevice != null)
            {
                onlineDevices.Insert(0, localDevice);
            }

            try
            {
                // 启动监听线程
                Log.Information($"开始监听端口：{ListenPort}");
                _listener = new UdpClient(ListenPort);
                var listenThread = new Thread(ListenForResponses);
                listenThread.IsBackground = true;
                listenThread.Start();

                // 发送广播
                await SendBroadcastMessage();
            }
            catch (Exception ex)
            {
                Log.Error($"Fail to detect devices, error: {ex.Message}");
            }
        }

        public async Task SendBroadcastMessage()
        {
            var broadcastIps = NetworkTool.GetBroadcastIp();
            foreach (var ipAddress in broadcastIps)
            {
                using (var broadcaster = new UdpClient())
                {
                    Log.Information($"发送UDP广播到子网：{ipAddress}与端口：{BroadcastPort}");
                    broadcaster.EnableBroadcast = true;
                    var broadcastIp = new IPEndPoint(ipAddress, BroadcastPort);
                    var message = Encoding.ASCII.GetBytes("STB_REQUEST|DISCOVERY");
                    await broadcaster.SendAsync(message, message.Length, broadcastIp);
                }
            }
        }

        private void ListenForResponses()
        {
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, ListenPort);

                while (true)
                {
                    var bytes = _listener.Receive(ref endPoint);
                    var message = Encoding.ASCII.GetString(bytes);

                    if (message.StartsWith("STB_RESPONSE"))
                    {
                        Log.Information($"接收到来自IP：{endPoint.Address.ToString()}的消息, 开始添加设备!");
                        var deviceInfo = message.Split('|');
                        if (deviceInfo.Length > 1)
                        {
                            var snCode = deviceInfo[1];
                            if (!string.IsNullOrEmpty(snCode))
                            {
                                Log.Information($"设备SN码：{snCode}");
                                var existDevice = onlineDevices.FirstOrDefault(c => c.SnCode == snCode);
                                if (existDevice != null && existDevice.DeviceViewModel != null && existDevice.DeviceViewModel.IsRealTimeConnected())
                                {
                                    Log.Information($"设备 IP：{endPoint.Address.ToString()} 已连接");
                                    InvokeDevicesChanged?.Invoke(this, null);
                                    continue;
                                }

                                if (existDevice != null)
                                {
                                    onlineDevices.Remove(existDevice);
                                }

                                var device = new InternetDevice
                                {
                                    SnCode = snCode,
                                    IpAddress = endPoint.Address.ToString(),
                                    Status = 0,
                                    IsInternet = true,
                                };
                                onlineDevices.Add(device);
                                InvokeDevicesChanged?.Invoke(this, null);
                                Log.Information($"添加SN码：{snCode}的设备成功，开始连接!");
                                ConnectDevice(device).Wait();
                            }
                        }
                    }

                    //System.Threading.Thread.Sleep(5000);
                    //var device = new InternetDevice
                    //{
                    //    SnCode = "test" + OnlineDevices.Count,
                    //    IpAddress = "1111",
                    //    Status = 0,
                    //    StatusText = GetStatus(0),
                    //    IsInternet = true,
                    //    TypeText = GetDeviceType(true)
                    //};
                    //OnlineDevices.Add(device);
                    //ConnectInternetDevice(device).GetAwaiter().OnCompleted(() =>
                    //{
                    //    InvokeDevicesChanged();
                    //});
                }
            }
            catch (SocketException)
            {
                // 正常退出时会触发
            }
            finally
            {
                Log.Error($"Listen is stopped");
            }
        }

        public async Task ConnectDevice(InternetDevice device)
        {
            if (device.DeviceViewModel == null || !device.DeviceViewModel.IsConnected || !device.DeviceViewModel.IsRealTimeConnected())
            {
                Log.Debug($"Start to connect device with IP {device.IpAddress}!");
                var ftpServer = GetFtpServer(device.IpAddress);
                var communication = new Communication(ftpServer, true);
                communication.Connect(device.IpAddress, "5001");
                int count = 5;
                while (communication.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
                {
                    await Task.Delay(500);
                    count--;
                }

                if (communication.netClient.State != Helpers.SocketClient.SocketState.Connected)
                {
                    Log.Error($"Fail to connect device with IP: {device.IpAddress}");
                    if (device.DeviceViewModel != null)
                    {
                        device.DeviceViewModel.ErrorMessage = "LanguageKey_Code_Device_Tooltip_100";
                    }
                }
                else
                {
                    var monitorService = Utility.GetService<IMonitorService>();
                    var userService = Utility.GetService<IUserService>();
                    var connectedDevice = (await monitorService.GetAll(new MonitorDto { SNumber = device.SnCode })).Data?.FirstOrDefault();
                    if (connectedDevice == null)
                    {
                        Log.Debug($"Start to Sync user from device with IP {device.IpAddress}!");
                        string path = CommunicationCmd.CmdSyncUser + "Login";
                        bool result = await communication.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
                        if (result)
                        {
                            var syncUsers = JsonConvert.DeserializeObject<UsersSync>(communication.SyncUserResult);
                            if (syncUsers != null)
                            {
                                foreach (var item in syncUsers.Users)
                                {                                    
                                    var response = await userService.Save(item.User);
                                    if (response.Code == 200)
                                    {
                                        Log.Debug($"Save user: {item.User.Account} to local db !");
                                        if (item.Monitor != null)
                                        {
                                            response = await monitorService.Save(item.Monitor.Monitor);
                                            if (response.Code == 200)
                                            {
                                                Log.Debug($"Save monitor: {item.Monitor.Monitor.SNumber} to local db !");
                                                device.DeviceViewModel = new DeviceViewModel();
                                                device.DeviceViewModel.Binding(item.Monitor.Monitor);
                                                device.DeviceViewModel.ConnectCommand.Execute(communication);
                                                device.UserAccount = item.User.Account;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        device.DeviceViewModel = new DeviceViewModel();
                        device.DeviceViewModel.Binding(connectedDevice);
                        device.DeviceViewModel.ConnectCommand.Execute(communication);
                        device.UserAccount = connectedDevice.UserAccount;
                    }

                    communication.StartHeart();

                    device.Status = 1;
                    Log.Debug($"Device with IP {device.IpAddress} is connected!");
                }
            }
        }

        private FtpServer GetFtpServer(string deviceIp)
        {
            var connection = App.ServicesProvider.GetRequiredService<FtpConnection>();
            var gatewayAddresses = NetworkTool.GetGatewayIp();
            Log.Information($"Gateway IP :{string.Join(";", gatewayAddresses)}");
            List<string> ipAddresses = new List<string>();
            if (gatewayAddresses.Contains(deviceIp))
            {
                Log.Information($"当前设备 IP {deviceIp} 为网关IP");
                ipAddresses = NetworkTool.GetLocalIPv4Address(deviceIp);
            }
            else
            {
                var prefix = string.Join(".", deviceIp.Split('.').Take(3));
                var gatewayAddress = gatewayAddresses.Find(c => c.StartsWith(prefix));
                if (!string.IsNullOrEmpty(gatewayAddress))
                {
                    Log.Information($"当前设备 IP {deviceIp} 的网关IP是: {gatewayAddress}");
                    ipAddresses = NetworkTool.GetLocalIPv4Address(gatewayAddress);
                }
            }

            if (ipAddresses.Count == 0)
            {
                Log.Debug($"未找到设备{deviceIp}对应的本地IP作为FTP服务器地址");
                ipAddresses.Add("127.0.0.1");
            }

            Log.Information($"Local IP :{string.Join(";", ipAddresses)}");
            var ftpServer = ftpServers.Find(c => c._Ip == ipAddresses[0] && c._port == connection.Port);
            if (ftpServer == null)
            {
                connection.IpAddress = ipAddresses[0];
                connection.Port = connection.Port + 1;
                ftpServer = new FtpServer(connection);

                ftpServers.Add(ftpServer);
            }

            return ftpServer;
        }

        public IEnumerable<InternetDevice> GetOnlineDevices()
        {
            return [.. onlineDevices];
        }

        public void StopDetect()
        {
            _listener.Close();
            _listener.Dispose();
            _listener = null;
            InvokeDevicesChanged = null;
        }
    }
}
