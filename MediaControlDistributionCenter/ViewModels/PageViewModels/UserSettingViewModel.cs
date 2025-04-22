using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserSettingViewModel : PageViewModel
    {
        public UserViewModel CurrentUser { get; set; }

        public UserViewModel LoginUser { get; set; }

        public bool IsShelf { get; set; } = true;

        public bool ShowNavigation { get; set; }

        public Thickness PageMargin => ShowNavigation ? new Thickness(20, 8, 20, 0) : new Thickness(0, 0, 0, 0);

        private const int BroadcastPort = 9876; // 广播端口
        private const int ListenPort = 9877;    // 接收回复端口
        private UdpClient _listener;

        private List<InternetDevice> detectDevices = new List<InternetDevice>();

        [ObservableProperty]
        private bool isScanning;

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        private ObservableCollection<object> roleList;

        [ObservableProperty]
        private string? pageType;

        [ObservableProperty]
        private string? detectStatus;

        [ObservableProperty]
        private string? oldPassword;

        [ObservableProperty]
        private string? newPassword;

        [ObservableProperty]
        private string? newPasswordConfirm;

        private readonly IUserService userService;
        private readonly DeviceManageViewModel deviceManageViewModel;

        public UserSettingViewModel(DeviceManageViewModel deviceManageViewModel)
        {
            this.userService = GetService<IUserService>();
            this.deviceManageViewModel = deviceManageViewModel;
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
            roleList = new ObservableCollection<object>(new List<RoleModel>
            {
                new RoleModel
                {
                    Role = RoleType.Admin.ToString().ToLower(),
                    RoleText = FindResource("LanguageKey_Code_Role_Admin")
                },
                new RoleModel
                {
                    Role = RoleType.Agent.ToString().ToLower(),
                    RoleText = FindResource("LanguageKey_Code_Role_Agent")
                },
                new RoleModel
                {
                    Role = RoleType.User.ToString().ToLower(),
                    RoleText = FindResource("LanguageKey_Code_Role_User")
                }
            });
        }

        public override void LoadData()
        {
            CurrentUser.LoadLogo();
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                var viewModel = ConnectedDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet)?.DeviceViewModel;
                if (viewModel != null && viewModel.IsConnected)
                {
                    await viewModel.VerifySnCodeCommand.ExecuteAsync(null);
                    if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                    {
                        ErrorMessage = viewModel.ErrorMessage;
                        await ShowConfirmDialogCommand.ExecuteAsync(null);
                        viewModel.ErrorMessage = null;
                        viewModel.DisconnectCommand.Execute(null);
                        return;
                    }

                    await viewModel.SendUserCommand.ExecuteAsync(null);
                    if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
                    {
                        ErrorMessage = viewModel.ErrorMessage;
                        await ShowConfirmDialogCommand.ExecuteAsync(null);
                        viewModel.ErrorMessage = null;
                        return;
                    }

                    if (viewModel.IsSendUserCompleted)
                    {
                        ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_129");
                        await ShowConfirmDialogCommand.ExecuteAsync(null);
                        viewModel.IsSendUserCompleted = false;
                    }
                }

                ErrorMessage = FindResource("LanguageKey_Code_Setting_Tooltip_104");
                await ShowConfirmDialogCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task ResetUser()
        {
            var response = await userService.GetById(CurrentUser.Id);
            if (response.Code == 200)
            {
                CurrentUser.Binding(response.Data!);
            }
        }

        [RelayCommand]
        private async Task DetectInternetDevices()
        {
            if (IsScanning) return;

            IsScanning = true;
            detectDevices.Clear();

            try
            {
                // 启动监听线程
                _listener = new UdpClient(ListenPort);
                var listenThread = new Thread(ListenForResponses);
                listenThread.IsBackground = true;
                listenThread.Start();

                // 发送广播
                using (var broadcaster = new UdpClient())
                {
                    broadcaster.EnableBroadcast = true;
                    var broadcastIp = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);
                    var message = Encoding.ASCII.GetBytes("STB_REQUEST|DISCOVERY");
                    await broadcaster.SendAsync(message, message.Length, broadcastIp);
                }

                DetectStatus = FindResource("LanguageKey_Code_Device_Tooltip_111");
            }
            catch (Exception ex)
            {
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_112");
                await ShowConfirmDialogCommand.ExecuteAsync(null);
                IsScanning = false;
            }
        }

        [RelayCommand]
        private async Task ConnectInternetDevice(InternetDevice device)
        {
            if (device.DeviceViewModel == null || !device.DeviceViewModel.IsConnected || !device.DeviceViewModel.IsRealTimeConnected())
            {
                var ftpServer = App.ServicesProvider.GetRequiredService<FtpServer>();
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
                    Log.Debug($"Device with IP {device.IpAddress} is connected!");
                    device.Status = 1;
                    device.StatusText = GetStatus(1);

                    var monitorService = GetService<IMonitorService>();
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
                        communication.StartHeart();
                    }
                }
            }            
        }

        [RelayCommand]
        private async Task DisconnectInternetDevice(InternetDevice device)
        {
            if (device.DeviceViewModel != null)
            {
                device.DeviceViewModel.DisconnectCommand.Execute(null);
                device.DeviceViewModel = null;
                device.Status = 0;
                device.StatusText = GetStatus(0);
            }

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task StopDetect()
        {
            IsScanning = false;
            _listener?.Close();
            DetectStatus = FindResource("LanguageKey_Code_Device_Tooltip_113");
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            CurrentUser.Password = NewPassword;

            var response = await userService.Save(CurrentUser.ToModel());
            if (response.Code == 200)
            {
                App.Current.Shutdown();
            }
        }

        [RelayCommand]
        private void CancelChangePassword()
        {
            OldPassword = null;
            NewPassword = null;
            NewPasswordConfirm = null;
        }

        private async void ListenForResponses()
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
                        var deviceInfo = message.Split('|');
                        if (deviceInfo.Length > 1)
                        {
                            var snCode = deviceInfo[1];
                            if (!ConnectedDevices.Any(c => c.SnCode == snCode))
                            {
                                var device = new InternetDevice
                                {
                                    SnCode = snCode,
                                    IpAddress = endPoint.Address.ToString(),
                                    Status = 0,
                                    StatusText = GetStatus(0),
                                    TypeText = GetDeviceType(true)
                                };
                                detectDevices.Add(device);
                            }
                        }
                    }

                    //System.Threading.Thread.Sleep(5000);
                    //var device = new InternetDevice
                    //{
                    //    SnCode = "test",
                    //    IpAddress = "1111",
                    //    Status = 0,
                    //    StatusText = GetStatus(0),
                    //    TypeText = GetDeviceType(true)
                    //};
                    //detectDevices.Add(device);

                    DetectStatus = string.Format(FindResource("LanguageKey_Code_Device_Tooltip_114"), detectDevices.Count);
                    var localDevice = ConnectedDevices.FirstOrDefault(c => c.DeviceViewModel != null && !c.DeviceViewModel.IsInternet);
                    if (localDevice != null)
                    {
                        detectDevices.Insert(0, localDevice);
                    }
                    ConnectedDevices = new ObservableCollection<InternetDevice>(detectDevices);

                    //await ConnectInternetDevice(device);
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
    }
}
