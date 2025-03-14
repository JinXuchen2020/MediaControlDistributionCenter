using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class LoginViewModel : PageViewModel
    { 
        [ObservableProperty]
        private UserViewModel currentUser;

        [ObservableProperty]
        private bool isLogin;

        private readonly IAuthService authService;
        private readonly IUserService userService;

        public LoginViewModel(IAuthService authService, IUserService userService)
        {
            this.authService = authService;
            this.userService = userService;
            currentUser = new UserViewModel();
        }

        [RelayCommand]
        private async Task Login(AccountDto request)
        {
            var resultResponse = await authService.Login(request);
            if (resultResponse.Code == 200)
            {
                var userString = resultResponse.Data!;
                var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
                if(connectionMode.Mode == "Local")
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

        public override void LoadData(long? groupId = null)
        {
            return;
        }
    }
}
