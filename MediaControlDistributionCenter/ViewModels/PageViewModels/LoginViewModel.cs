using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class LoginViewModel : PageViewModel
    { 
        [ObservableProperty]
        private UserViewModel currentUser;

        [ObservableProperty]
        private bool isLogin;

        [ObservableProperty]
        private ConnectionMode connectionMode;

        [ObservableProperty]
        private ObservableCollection<string> ipAddresses;

        [ObservableProperty]
        private string selectedIpAddress;

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
            this.ipAddresses = new ObservableCollection<string>(new List<string> { NetworkTool.GetGatewayIp() });
            selectedIpAddress = ipAddresses.First();
            this.communication = communication;
        }

        [RelayCommand]
        private async Task Login(AccountDto request)
        {
            var resultResponse = await authService.Login(request);
            if (resultResponse.Code == 200)
            {
                var userString = resultResponse.Data!;
                var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
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
            }
        }

        [RelayCommand]
        private async Task Connect()
        {
            communication.Connect(SelectedIpAddress, "5001");
            int count = 1;
            while (communication.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
            {
                Thread.Sleep(500);
                count--;
            }
            if (communication.netClient.State != Helpers.SocketClient.SocketState.Connected)
            {
                MessageBox.Show("无法连接机顶盒!");
                return;
            }

            string path = CommunicationCmd.CmdSyncUser;
            bool result = await communication.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                MessageBox.Show($"命令处理成功!当前用户列表为:{communication.SyncUserResult}");
                var syncUsers = JsonConvert.DeserializeObject<UsersSync>(communication.SyncUserResult);
                if (syncUsers != null)
                {
                    foreach (var item in syncUsers.Users) 
                    {
                        await userService.Save(item.User);
                        if (item.UserGroup != null)
                        {
                            await userGroupService.Save(item.UserGroup);
                        }

                        if (item.Monitor != null)
                        {
                            await monitorService.Save(item.Monitor.Monitor);
                            foreach (var program in item.Monitor.Programs)
                            {
                                await programService.Save(program);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("命令无法被处理!");
            }
        }

        public override void LoadData(long? groupId = null)
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
    }
}
