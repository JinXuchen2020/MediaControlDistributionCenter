using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dm.filter;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserViewModel : ObservableValidator
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]
        private string role;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string agentId;

        [ObservableProperty]
        private string group;

        [ObservableProperty]
        [Required]
        [CustomValidation(typeof(UserViewModel), nameof(ValidateAccount))]
        private string account;

        [ObservableProperty]
        [Required]
        private string name;

        [ObservableProperty]
        private string region;

        [ObservableProperty]
        [Required]
        private string password;

        [ObservableProperty]
        public string? timeZone;

        [ObservableProperty]
        public string? logo;

        [ObservableProperty]
        public BitmapImage? logoThumbnail;

        [ObservableProperty]
        public string? companyName;

        [ObservableProperty]
        public string? tagLine;

        [ObservableProperty]
        public bool isUpload;

        [ObservableProperty]
        private bool isSelected;        

        [ObservableProperty]
        private ObservableCollection<UserGroupViewModel> groups;

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }

        [RelayCommand]
        private void Reset()
        {
            Role = string.Empty;
            GroupId = null;
            Account = string.Empty;
            Name = string.Empty;
            Region = string.Empty;
            Password = string.Empty;
        }

        [RelayCommand]
        private void Submit()
        {
            ValidateAllProperties();
        }

        public UserDto ToModel()
        {
            return new UserDto
            {
                Id = Id,
                Company = Name,
                Account = Account,
                Region = Region,
                Password = Password,
                UserGroupId = GroupId,
                AgentAccount = AgentId,
                Role = Role,
                LogoSrc = Logo,
                TimeZone = TimeZone,
            };
        }

        public void Binding(UserDto model)
        {
            Id = model.Id;
            Role = model.Role;
            GroupId = model.UserGroupId;
            AgentId = model.AgentAccount;
            Group = model.UserGroupName ?? "未分组";
            Account = model.Account;
            Name = model.Company;
            Region = model.Region;
            Password = model.Password;
            Logo = model.LogoSrc;
            TimeZone = model.TimeZone;
            LogoThumbnail = GetThumbnail();
        }

        public BitmapImage? GetThumbnail()
        {
            if (!string.IsNullOrEmpty(Logo) && File.Exists(Logo))
            {
                return new BitmapImage(new Uri(Logo));
            }

            return null;
        }

        public static ValidationResult ValidateAccount(string account, ValidationContext context)
        {
            UserViewModel instance = (UserViewModel)context.ObjectInstance;
            var userService = App.ServicesProvider.GetRequiredService<IUserService>();
            var response = userService.GetAll(new UserDto { Account = account }).GetAwaiter().GetResult().Data?.ToList() ?? new List<UserDto>();
            bool isValid = response.Where(c => c.Id != instance.Id).Count() == 0;

            if (isValid)
            {
                return ValidationResult.Success;
            }

            var erroMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Totip_410");
            return new(erroMessage);
        }
    }
}
