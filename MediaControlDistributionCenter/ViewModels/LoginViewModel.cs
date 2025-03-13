using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class LoginViewModel : PageViewModel
    { 
        [ObservableProperty]
        private UserViewModel currentUser;

        [ObservableProperty]
        private bool isLogin;

        private readonly IAuthService authService;

        public LoginViewModel(IAuthService authService)
        {
            this.authService = authService;
            currentUser = new UserViewModel();
        }

        [RelayCommand]
        private async Task Login(AccountDto request)
        {
            var resultResponse = await authService.Login(request);
            if (resultResponse.Code == 200)
            {
                var userString = resultResponse.Data!;
                var loginUser = JsonConvert.DeserializeObject<UserDto>(userString);
                CurrentUser.Binding(loginUser!);
                IsLogin = true;
            }
        }
    }
}
