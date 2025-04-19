using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
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
        private ObservableCollection<InternetDevice> devices;

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
            devices = new ObservableCollection<InternetDevice>();
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
                var viewModel = ConnectedDevice;
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
            Devices.Clear();

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
                        var deviceInfo = message.Split('|');
                        if (deviceInfo.Length > 1)
                        {
                            var device = new InternetDevice
                            {
                                SnCode = deviceInfo.Last(),
                                IpAddress = endPoint.Address.ToString(),
                                Status = 0,
                                StatusText = GetStatus(0)
                            };
                            detectDevices.Add(device);
                        }
                    }
                }
            }
            catch (SocketException)
            {
                // 正常退出时会触发
            }
            finally
            {
                string message = FindResource("LanguageKey_Code_Device_Tooltip_114");
                DetectStatus = string.Format(message, detectDevices.Count);
                Devices = new ObservableCollection<InternetDevice>(detectDevices);
                IsScanning = false;
            }
        }

        public string GetStatus(int status)
        {
            return status == 1 ? FindResource("LanguageKey_Code_Connected") : FindResource("LanguageKey_Code_Disconnected");
        }
    }

    public class InternetDevice
    {
        public string SnCode { get; set; }

        public string IpAddress { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }
    }
}
