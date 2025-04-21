using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dm.filter;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenCvSharp;
using Serilog;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class LoginViewModel : PageViewModel
    { 
        [ObservableProperty]
        private UserViewModel currentUser;

        [ObservableProperty]
        private string inputAccount;

        [ObservableProperty]
        private bool isLogin;

        [ObservableProperty]
        private bool isSync;

        [ObservableProperty]
        private bool isSyncing;

        [ObservableProperty]
        private ConnectionMode connectionMode;

        [ObservableProperty]
        private ObservableCollection<string> ipAddresses;

        [ObservableProperty]
        private ObservableCollection<string> syncUsers;

        [ObservableProperty]
        private string? selectedIpAddress;

        [ObservableProperty]
        public BitmapImage logoThumbnail;

        [ObservableProperty]
        public string? slogan;

        [ObservableProperty]
        public bool hasDevices;

        private IAuthService authService;
        private IUserService userService;
        private IUserGroupService userGroupService;
        private IMonitorService monitorService;
        private IProgramService programService;
        private readonly Communication communication;

        public LoginViewModel(ConnectionMode connectionMode, Communication communication)
        {
            this.authService = GetService<IAuthService>();
            this.userService = GetService<IUserService>();
            this.userGroupService = GetService<IUserGroupService>();
            this.monitorService = GetService<IMonitorService>();
            this.programService = GetService<IProgramService>();
            currentUser = new UserViewModel();
            this.connectionMode = connectionMode;
            this.ipAddresses = new ObservableCollection<string>(NetworkTool.GetGatewayIp());
            this.communication = communication;
            this.syncUsers = new ObservableCollection<string>();
            RefreshLogo();
        }

        public async Task DetectConnectedDevice()
        {
            IpAddresses = new ObservableCollection<string>(NetworkTool.GetGatewayIp());
            if (!IpAddresses.Contains(communication.IpAddr) || communication.netClient.State != Helpers.SocketClient.SocketState.Connected)
            {
                foreach (var address in IpAddresses)
                {
                    communication.Connect(address, "5001");
                    int count = 5;
                    while (communication.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
                    {
                        await Task.Delay(500);
                        count--;
                    }

                    if (communication.netClient.State == Helpers.SocketClient.SocketState.Connected)
                    {
                        break;
                    }
                }
            }

            if (SelectedIpAddress != communication.IpAddr && communication.netClient.State == Helpers.SocketClient.SocketState.Connected)
            {
                HasDevices = true;
                SelectedIpAddress = communication.IpAddr;
                Log.Debug($"Device with IP {SelectedIpAddress} is connected!");
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
                                        var device = ConnectedDevices.FirstOrDefault(c => c.SnCode == item.Monitor.Monitor.SnCode);
                                        if (device == null)
                                        {
                                            device = new InternetDevice()
                                            {
                                                SnCode = item.Monitor.Monitor.SnCode,
                                                IpAddress = SelectedIpAddress,
                                                Status = 1,
                                                StatusText = GetStatus(1)
                                            };

                                            ConnectedDevices.Add(device);
                                        } 
                                        device.DeviceViewModel = new DeviceViewModel();
                                        device.DeviceViewModel.Binding(item.Monitor.Monitor);
                                        device.DeviceViewModel.ConnectCommand.Execute(communication);
                                    }
                                }
                            }
                        }
                    }

                    this.IsSync = true;
                    RefreshLogo();
                }
            }
        }

        [RelayCommand]
        private async Task Login(AccountDto request)
        {
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty(request.Password))
            {
                ErrorMessage = FindResource("LanguageKey_Code_Login_Tooltip_103");
            }
            else if (connectionMode.Mode == "Local" && !IsSync)
            {
               ErrorMessage = FindResource("LanguageKey_Code_Login_Tooltip_100"); //"请先同步机顶盒信息！";
            }
            //else if (connectionMode.Mode == "Local" && (!this.SyncUsers.Contains(request.Account) || request.Account == "admin"))
            //{
            //    ErrorMessage = FindResource("LanguageKey_Code_Login_Tooltip_101"); // "该账号不可用！";
            //}
            else
            {
                var resultResponse = await authService.Login(request);
                if (resultResponse.Code == 200)
                {
                    var userString = resultResponse.Data!;
                    if (connectionMode.Mode == "Local" || string.IsNullOrEmpty(connectionMode.ServiceUri))
                    {
                        var loginUser = JsonConvert.DeserializeObject<UserDto>(userString);
                        CurrentUser.Binding(loginUser!);
                        IsLogin = true;
                    }
                    else
                    {
                        connectionMode.RemoteToken = userString.Split(" ")[1];
                        var userResponse = await userService.GetAll(new UserDto { Account = request.Account });
                        if (userResponse.Code == 200)
                        {
                            var userResult = userResponse.Data?.FirstOrDefault();
                            if (userResult != null)
                            {
                                CurrentUser.Binding(userResult);
                                IsLogin = true;
                            }
                        }
                    }

                    if (ConnectionMode.Mode == "Local")
                    {
                        communication.StartHeart();
                    }
                }
                else
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Login_Tooltip_103");  //"账号或密码错误！";
                }
            }

            if (!IsLogin)
            {
                var dialog = new ResultConfirmDialog(this);
                await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.LoginDialogHostId);
            }
        }

        [RelayCommand]
        private async Task Connect()
        {
            if (IsSyncing)
            {
                return;
            }
            SyncUsers = new ObservableCollection<string>();
            IsSyncing = true;
            if (!string.IsNullOrEmpty(SelectedIpAddress) && SelectedIpAddress != communication.IpAddr)
            {
                communication.Connect(SelectedIpAddress, "5001");
                int count = 5;
                while (communication.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
                {
                    await Task.Delay(500);
                    count--;
                }

                if (communication.netClient.State != Helpers.SocketClient.SocketState.Connected)
                {
                    ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_100");// MessageBox.Show("无法连接机顶盒!");
                }
                else
                {
                    Log.Debug($"Device with IP {SelectedIpAddress} is connected!");
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
                                            var device = ConnectedDevices.FirstOrDefault(c => c.SnCode == item.Monitor.Monitor.SnCode);
                                            if (device == null)
                                            {
                                                device = new InternetDevice()
                                                {
                                                    SnCode = item.Monitor.Monitor.SnCode,
                                                    IpAddress = SelectedIpAddress,
                                                    Status = 1,
                                                    StatusText = GetStatus(1)
                                                };

                                                ConnectedDevices.Add(device);
                                            }
                                            device.DeviceViewModel = new DeviceViewModel();
                                            device.DeviceViewModel.Binding(item.Monitor.Monitor);
                                            device.DeviceViewModel.ConnectCommand.Execute(communication);
                                            Log.Debug($"Current Connected Device is {device.DeviceViewModel.Name}");
                                        }
                                    }

                                    if (item.User.Role != RoleType.Admin.ToString().ToLower())
                                    {
                                        this.SyncUsers.Add(item.User.Account);
                                    }
                                }
                            }
                        }

                        this.IsSync = true;
                        RefreshLogo();
                        IsSyncing = false;
                    }
                    else
                    {
                        ErrorMessage = $"{CommunicationCmd.CmdSyncUser} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                    }
                }

                var dialog = new ResultConfirmDialog(this);
                await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.LoginDialogHostId);
                IsSyncing = false;
            }            
        }

        public override void LoadData()
        {
            return;
        }

        public void RefreshService()
        {
            this.userService = GetService<IUserService>();
            this.authService = GetService<IAuthService>();
            this.userGroupService = GetService<IUserGroupService>();
            this.monitorService = GetService<IMonitorService>();
            this.programService = GetService<IProgramService>();
        }

        private void RefreshLogo()
        {
            BitmapImage? result = null;
            if (ConnectionMode.Mode == "Local" && this.SyncUsers.Count > 0)
            {
                var user = userService.GetAll(new UserDto { Account = this.SyncUsers.Last() }).GetAwaiter().GetResult().Data?.FirstOrDefault();
                if (user != null) 
                {
                    if (!string.IsNullOrEmpty(user.LogoSrc))
                    {
                        try
                        {
                            UserViewModel viewModel = new UserViewModel();
                            viewModel.Binding(user);
                            viewModel.LoadLogo();
                            result = viewModel.LogoThumbnail;
                        }
                        catch
                        {
                            result = null;
                        }
                    }

                    Slogan = user.TagLine;
                }
            }

            if (result == null)
            {
                var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/assets-0004.png", UriKind.Absolute);
                var resourceStream = Application.GetResourceStream(uri);
                result = new BitmapImage();
                result.BeginInit();
                result.StreamSource = resourceStream.Stream;
                result.EndInit();
            }

            LogoThumbnail = result;
            Slogan ??= FindResource("LanguageKey_Code_DefaultSlogan");
            RegisterLanguageProperty(this.GetType(), nameof(Slogan));            
        }
    }
}
