using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserViewModel : DataViewModel<UserDto>
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]        
        private string role;

        [ObservableProperty]
        private long? adminUserGroupId;

        [ObservableProperty]
        private long? agentUserGroupId;

        [ObservableProperty]
        private string? agentId;

        [ObservableProperty]
        private string? group;

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
        private int status;

        [ObservableProperty]
        [Required]
        private string password;

        [ObservableProperty]
        public string? timeZone;

        [ObservableProperty]
        public string? logo;

        [ObservableProperty]
        public string? logoFileName;

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
        private long? selectedGroupId;

        [ObservableProperty]
        private ObservableCollection<UserGroupViewModel> groups;

        [ObservableProperty]
        private ObservableCollection<UserViewModel> agents;

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> timeZoneInfos;

        [ObservableProperty]
        private ObservableCollection<object> roleList;

        public UserViewModel()
        {
            timeZoneInfos = new ObservableCollection<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }

        [RelayCommand]
        private void Reset()
        {
            Logo = null;
            LogoThumbnail = null;
            TagLine = null;
            TimeZone = null;
            CompanyName = null;
            IsUpload = false;
        }

        public override UserDto ToModel()
        {
            return new UserDto
            {
                Id = Id,
                Company = Name,
                Account = Account,
                Region = Region,
                Password = Password,
                AdminUserGroupId = AdminUserGroupId,
                AgentUserGroupId = AgentUserGroupId,
                AgentAccount = AgentId,
                Role = Role,
                LogoSrc = LogoFileName,
                LogoFileName = LogoFileName,
                TimeZone = TimeZone,
                Status = Status,
                TagLine = TagLine,
            };
        }

        public override void Binding(UserDto model, bool isSelected = false)
        {
            Id = model.Id;
            Role = model.Role.ToLower();
            AdminUserGroupId = model.AdminUserGroupId;
            AgentUserGroupId = model.AgentUserGroupId;
            AgentId = model.AgentAccount;
            Group = model.UserGroupName ?? FindResource("LanguageKey_Code_NoGroup");
            Account = model.Account;
            Name = model.Company;
            Region = model.Region;
            Password = model.Password;
            LogoFileName = model.LogoFileName;
            //Logo = DownloadLogo();
            TimeZone = model.TimeZone;
            //LogoThumbnail = GetThumbnail();
            IsSelected = isSelected;
            IsUpload = Logo != null;
            TagLine = model.TagLine;
        }

        public void LoadLogo()
        {
            Logo = Logo ?? DownloadLogo();
            LogoThumbnail = LogoThumbnail ?? GetThumbnail();
            IsUpload = Logo != null;
        }

        public string? DownloadLogo()
        {
            if (string.IsNullOrEmpty(LogoFileName)) 
            {
                return null;
            }

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, LogoFileName!);
            if (!File.Exists(filePath)) 
            {
                var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();
                var result = ftpClient.DownloadFile(LogoFileName!).GetAwaiter().GetResult();
                if (!result)
                {
                    filePath = null;
                }
            }

            return filePath;
            
        }

        public BitmapImage? GetThumbnail()
        {            
            if (!string.IsNullOrEmpty(Logo))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    image.UriSource = new Uri(Logo);
                    image.EndInit();

                    return image;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public string? GetImageData(string? content)
        {
            byte[]? imageData = null;
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    imageData = Convert.FromBase64String(content);
                }
                catch
                {
                    if (File.Exists(content))
                    {
                        imageData = File.ReadAllBytes(content);
                    }
                }
            }

            return imageData != null ? Convert.ToBase64String(imageData) : null;
        }

        public static ValidationResult ValidateAccount(string account, ValidationContext context)
        {
            UserViewModel instance = (UserViewModel)context.ObjectInstance;
            var userService = GetService<IUserService>();
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

    public class RoleModel
    {
        public string Role { get; set; }

        public string RoleText { get; set; }
    }
}
