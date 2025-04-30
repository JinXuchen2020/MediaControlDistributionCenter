using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class PageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ConnectionMode connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
        
        [ObservableProperty]
        private static bool isDeviceConnected = App.ServicesProvider.GetRequiredService<Communication>().netClient.IsConnected;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string? searchString;

        [ObservableProperty]
        private bool? canDelete;

        private static Dictionary<Type, List<string>> languagePropertyCache = new Dictionary<Type, List<string>>();

        private static Dictionary<Type, List<string>> devicesChangedRegisterActions = new Dictionary<Type, List<string>>();

        public static List<InternetDevice> OnlineDevices = new List<InternetDevice>();

        public static List<FtpServer> FtpServers = new List<FtpServer>();

        private const int BroadcastPort = 5001;//9876; // 广播端口
        private const int ListenPort = 5001;//9877;    // 接收回复端口
        private UdpClient _listener;

        [ObservableProperty]
        private bool isScanning;

        public virtual void LoadData()
        {

        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.ErrorMessageBoxId);
        }

        [RelayCommand]
        private async Task Search()
        {
            await SearchContent();
        }

        protected T GetService<T>() where T : class
        {
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            switch (connectionMode.Mode)
            {
                case "Local":
                    return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                case "Remote":
                    if(string.IsNullOrEmpty(connectionMode.ServiceUri))
                    {
                        return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                    }
                    else
                    {
                        return App.ServicesProvider.GetServices<T>().First(c => !c.GetType().Name.EndsWith("Local"));
                    }
                default:
                    throw new ArgumentException("未知的服务名称");
            }
        }

        protected string FindResource(string key)
        {
            return (string)LanguageTool.Instance.FindResource(key);
        }

        protected virtual async Task SearchContent()
        {
            await Task.CompletedTask;
        }

        protected void RegisterLanguageProperty(Type parentType, string propertyName)
        {
            if (!languagePropertyCache.ContainsKey(parentType))
            {
                languagePropertyCache.Add(parentType, new List<string> { propertyName });
            }
            else if (!languagePropertyCache[parentType].Contains(propertyName))
            {
                languagePropertyCache[parentType].Add(propertyName);
            }
        }

        protected void RegisterDevicesChangedAction(Type parentType, string propertyName)
        {
            if (!devicesChangedRegisterActions.ContainsKey(parentType))
            {
                devicesChangedRegisterActions.Add(parentType, new List<string> { propertyName });
            }
            else if (!devicesChangedRegisterActions[parentType].Contains(propertyName))
            {
                devicesChangedRegisterActions[parentType].Add(propertyName);
            }
        }

        public void InvokeDevicesChanged()
        {
            foreach (var item in devicesChangedRegisterActions)
            {
                foreach (var proName in item.Value)
                {
                    var typeObj = App.ServicesProvider.GetRequiredService(item.Key);
                    var property = item.Key.GetProperty(proName);
                    if (property != null)
                    {
                        var propertyValue = (string)property.GetValue(typeObj)!;
                        property.SetValue(typeObj, LanguageTool.Instance.GetResourceTextValue(propertyValue));
                    }
                    else
                    {
                        var method = item.Key.GetMethod(proName);
                        var parameters = method?.GetParameters();
                        if (parameters != null)
                        {
                            var parameterValues = new List<object?>();
                            foreach (var parameter in parameters)
                            {
                                parameterValues.Add(Activator.CreateInstance(parameter.ParameterType));
                            }
                            method?.Invoke(typeObj, [.. parameterValues]);
                        }
                        else
                        {
                            method?.Invoke(typeObj, null);
                        }
                    }
                }
            }
        }

        public void TranslateLanguageProperties()
        {
            foreach (var item in languagePropertyCache)
            {
                foreach (var proName in item.Value)
                {
                    var typeObj = App.ServicesProvider.GetRequiredService(item.Key);
                    var property = item.Key.GetProperty(proName);
                    if (property != null)
                    {
                        var propertyValue = (string)property.GetValue(typeObj)!;
                        if (propertyValue != null)
                        {
                            property.SetValue(typeObj, LanguageTool.Instance.GetResourceTextValue(propertyValue));
                        }
                    }
                    else
                    {
                        var method = item.Key.GetMethod(proName);
                        var parameters = method?.GetParameters();
                        if (parameters != null)
                        {
                            var parameterValues = new List<object?>();
                            foreach (var parameter in parameters)
                            {
                                parameterValues.Add(Activator.CreateInstance(parameter.ParameterType));
                            }
                            method?.Invoke(typeObj, [.. parameterValues]);
                        }
                        else
                        {
                            method?.Invoke(typeObj, null);
                        }
                    }
                }
            }
        }

        //protected async Task DetectCommunication(string userAccount)
        //{
        //    var client = App.ServicesProvider.GetRequiredService<Communication>();
        //    var localDevice = OnlineDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet);
        //    var localDeviceModel = localDevice?.DeviceViewModel;
        //    if (localDevice != null && localDeviceModel != null && localDeviceModel.UserId == userAccount && localDeviceModel.SelectedIpAddress == client.IpAddr && client.netClient.State == Helpers.SocketClient.SocketState.Connected)
        //    {
        //        return;
        //    }

        //    var ipAddress = NetworkTool.GetGatewayIp();
        //    foreach (var address in ipAddress)
        //    {
        //        client.Connect(address, "5001");
        //        int count = 10;
        //        while (client.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
        //        {
        //            await Task.Delay(500);
        //            count--;
        //        }

        //        if (client.netClient.State == Helpers.SocketClient.SocketState.Connected)
        //        {
        //            break;
        //        }
        //    }

        //    if (client.netClient.State != Helpers.SocketClient.SocketState.Connected)
        //    {
        //        if (localDevice != null)
        //        {
        //            localDevice.DeviceViewModel = null;
        //        }
        //        return;
        //    }

        //    string path = CommunicationCmd.CmdSyncSnCode + "Connect";
        //    bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
        //    if (!result)
        //    {
        //        ErrorMessage = $"{CommunicationCmd.CmdSyncSnCode} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
        //        await ShowConfirmDialogCommand.ExecuteAsync(null);
        //        return;
        //    }

        //    var snCode = client.SyncSnCodeResult ?? string.Empty;

        //    var monitorService = GetService<IMonitorService>();
        //    var connectedDevice = monitorService.GetAll(new MonitorDto { SnCode = snCode }).GetAwaiter().GetResult().Data?.FirstOrDefault();
        //    if (connectedDevice != null)
        //    {
        //        if (localDevice == null)
        //        {
        //            localDevice = new InternetDevice
        //            {
        //                SnCode = snCode,
        //                IpAddress = client.IpAddr,
        //                Status = 1,
        //                StatusText = GetStatus(1),
        //                IsInternet = false,
        //                TypeText = GetDeviceType(false)
        //            };

        //            OnlineDevices.Add(localDevice);
        //        }
        //        else
        //        {
        //            if (localDevice.IpAddress != client.IpAddr)
        //            {
        //                OnlineDevices.Remove(localDevice);
        //                localDevice = new InternetDevice
        //                {
        //                    SnCode = snCode,
        //                    IpAddress = client.IpAddr,
        //                    Status = 1,
        //                    StatusText = GetStatus(1),
        //                    IsInternet = false,
        //                    TypeText = GetDeviceType(false)
        //                };

        //                OnlineDevices.Add(localDevice);
        //            }
        //        }

        //        localDevice.DeviceViewModel = new DeviceViewModel();
        //        localDevice.DeviceViewModel.Binding(connectedDevice);
        //        localDevice.DeviceViewModel.ConnectCommand.Execute(client);
        //        client.StartHeart();

        //        InvokeDevicesChanged();
        //    }
        //}

        [RelayCommand]
        private async Task DetectInternetDevices()
        {
            if (IsScanning) return;

            IsScanning = true;
            var localDevice = OnlineDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet);
            OnlineDevices.Clear();
            if (localDevice != null)
            {
                OnlineDevices.Insert(0, localDevice);
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

                //DetectStatus = FindResource("LanguageKey_Code_Device_Tooltip_111");
            }
            catch (Exception ex)
            {
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_112");
                await ShowConfirmDialogCommand.ExecuteAsync(null);
                IsScanning = false;
            }
        }

        [RelayCommand]
        private async Task SendBroadcastMessage()
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

                while (IsScanning)
                {
                    var bytes = _listener.Receive(ref endPoint);
                    var message = Encoding.ASCII.GetString(bytes);

                    if (message.StartsWith("STB_RESPONSE"))
                    {
                        Log.Information($"接收到来自IP：{endPoint.Address.ToString()}, 开始添加设备!");
                        var deviceInfo = message.Split('|');
                        if (deviceInfo.Length > 1)
                        {
                            var snCode = deviceInfo[1];
                            if (!string.IsNullOrEmpty(snCode))
                            {
                                Log.Information($"设备SN码：{snCode}");
                                var existDevice = OnlineDevices.FirstOrDefault(c => c.SnCode == snCode);
                                if (existDevice != null && existDevice.DeviceViewModel != null && existDevice.DeviceViewModel.IsRealTimeConnected()) 
                                {
                                    Log.Information($"设备 IP：{endPoint.Address.ToString()} 已连接");
                                    InvokeDevicesChanged();
                                    continue;
                                }

                                if (existDevice != null)
                                {
                                    OnlineDevices.Remove(existDevice);
                                }

                                var device = new InternetDevice
                                {
                                    SnCode = snCode,
                                    IpAddress = endPoint.Address.ToString(),
                                    Status = 0,
                                    StatusText = GetStatus(0),
                                    IsInternet = true,
                                    TypeText = GetDeviceType(true)
                                };
                                OnlineDevices.Add(device);
                                InvokeDevicesChanged();
                                Log.Information($"添加SN码：{snCode}的设备成功，开始连接!");
                                ConnectInternetDevice(device).Wait();
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
                IsScanning = false;
            }
        }

        [RelayCommand]
        private async Task ConnectInternetDevice(InternetDevice device)
        {
            if (device.DeviceViewModel == null || !device.DeviceViewModel.IsConnected || !device.DeviceViewModel.IsRealTimeConnected())
            {
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
                    ErrorMessage = (string)FindResource("LanguageKey_Code_Device_Tooltip_100");// MessageBox.Show("无法连接机顶盒!");
                    await ShowConfirmDialogCommand.ExecuteAsync(null);
                    device.DeviceViewModel = null;
                }
                else
                {
                    var monitorService = GetService<IMonitorService>();
                    var userService = GetService<IUserService>();
                    var connectedDevice = monitorService.GetAll(new MonitorDto { SnCode = device.SnCode }).GetAwaiter().GetResult().Data?.FirstOrDefault();
                    if (connectedDevice == null)
                    {
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
                                        if (item.Monitor != null)
                                        {
                                            response = await monitorService.Save(item.Monitor.Monitor);
                                            if (response.Code == 200)
                                            {
                                                device.DeviceViewModel = new DeviceViewModel();
                                                device.DeviceViewModel.Binding(item.Monitor.Monitor);
                                                device.DeviceViewModel.ConnectCommand.Execute(communication);
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
                    }

                    communication.StartHeart();

                    device.Status = 1;
                    device.StatusText = GetStatus(1);
                    Log.Debug($"Device with IP {device.IpAddress} is connected!");
                }
            }
        }

        public FtpServer GetFtpServer(string deviceIp)
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
            var ftpServer = FtpServers.Find(c => c._Ip == ipAddresses[0] && c._port == connection.Port);
            if (ftpServer == null)
            {
                connection.IpAddress = ipAddresses[0];
                connection.Port = connection.Port + 1;
                ftpServer = new FtpServer(connection);

                FtpServers.Add(ftpServer);
            }

            return ftpServer;
        }

        public string GetStatus(int status)
        {
            return status == 1 ? FindResource("LanguageKey_Code_Connected") : FindResource("LanguageKey_Code_Disconnected");
        }

        public string GetDeviceType(bool isInternet)
        {
            return isInternet ? FindResource("LanguageKey_Code_Device_Tooltip_118") : FindResource("LanguageKey_Code_Device_Tooltip_119");
        }
    }
}
