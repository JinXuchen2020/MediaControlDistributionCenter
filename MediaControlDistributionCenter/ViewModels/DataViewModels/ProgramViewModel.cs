using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class ProgramViewModel : DataViewModel<ProgramDto>
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]
        [Required(ErrorMessage = "请填写节目名称!")]
        [CustomValidation(typeof(ProgramViewModel), nameof(ValidateAccount))]
        private string name;

        [ObservableProperty]
        [Required(ErrorMessage ="请选择节目类型!")]
        private string type;

        [ObservableProperty]
        private string resolution;

        [ObservableProperty]
        [Required(ErrorMessage = "请填写节目高度!")]
        private string width;

        [ObservableProperty]
        [Required(ErrorMessage = "请填写节目宽度!")]
        private string height;

        [ObservableProperty]
        private double? size;

        [ObservableProperty]
        private int screensCount;

        [ObservableProperty]
        private string lastUpdatedTime;

        [ObservableProperty]
        private string createdSource;

        [ObservableProperty]
        private string createdSourceText;

        [ObservableProperty]
        private int status;

        [ObservableProperty]
        private string statusText;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string group;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private string rackingBtnContent;

        [ObservableProperty]
        private int? playCountPerHour;

        [ObservableProperty]
        private bool isHasValidity;

        [ObservableProperty]
        private DateTime? validStartDate;

        [ObservableProperty]
        private DateTime? validEndDate;

        [ObservableProperty]
        private BitmapImage? thumbnail;

        public override ProgramDto ToModel()
        {
            return new ProgramDto
            {
                Id = Id,
                Name = Name,
                MediaType = Type,
                Resolution = Resolution,
                Size = Size,
                MonitorCount = ScreensCount,
                LastUpdatedTime = LastUpdatedTime,
                CreatedSource = CreatedSource,
                Status = Status,
                GroupId = GroupId,
                UserAccount = UserId,
                PlayCountPerHour = PlayCountPerHour,
                IsHasValidity = IsHasValidity,
                ValidStartDate = ValidStartDate?.ToString("yyyy-MM-dd"),
                ValidEndDate = ValidEndDate?.ToString("yyyy-MM-dd"),
            };
        }

        public override void Binding(ProgramDto model, bool isSelected = false)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.MediaType;
            Resolution = model.Resolution;
            Width = string.IsNullOrEmpty(model.Resolution) ? "" : model.Resolution.Split("*")[0];
            Height = string.IsNullOrEmpty(model.Resolution) ? "" : model.Resolution.Split("*")[1];
            Size = model.Size;
            ScreensCount = model.MonitorCount;
            LastUpdatedTime = model.LastUpdatedTime;
            CreatedSource = model.CreatedSource;
            CreatedSourceText = model.CreatedSource == "user" ? FindResource("LanguageKey_Code_Role_User") : FindResource("LanguageKey_Code_Role_Admin");
            Status = model.Status;
            StatusText = GetStatus();
            GroupId = model.GroupId;
            UserId = model.UserAccount;
            IsSelected = isSelected;
            Group = model.ProgramGroupName ?? FindResource("LanguageKey_Code_NoGroup");
            RackingBtnContent = model.Status == 1 ? FindResource("LanguageKey_Code_OffShelf") : FindResource("LanguageKey_Code_OnShelf");
            PlayCountPerHour = model.PlayCountPerHour;
            IsHasValidity = model.IsHasValidity;
            ValidStartDate = string.IsNullOrEmpty(model.ValidStartDate) ? null : DateTime.Parse(model.ValidStartDate);
            ValidEndDate = string.IsNullOrEmpty(model.ValidEndDate) ? null : DateTime.Parse(model.ValidEndDate);
        }
        
        
        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }

        public static ValidationResult ValidateAccount(string name, ValidationContext context)
        {
            ProgramViewModel instance = (ProgramViewModel)context.ObjectInstance;
            var programService = Utility.GetService<IProgramService>();
            var response = Task.Run(async () => await programService.GetAll(new ProgramDto { UserAccount = instance.UserId, Name = name })).Result?.Data?.ToList() ?? new List<ProgramDto>();
            bool isValid = response.Where(c => c.Id != instance.Id).Count() == 0;

            if (isValid)
            {
                return ValidationResult.Success;
            }

            var errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Program_Tooltip_113");
            return new(errorMessage);
        }

        public string GetStatus()
        {
            var result = string.Empty;
            switch (Status)
            {
                case 0:
                    result = FindResource("LanguageKey_Code_Program_Tooltip_115");
                    break;
                case 1:
                    result = FindResource("LanguageKey_Code_Program_Tooltip_116");
                    break;
                case 2:
                    result = FindResource("LanguageKey_Code_Program_Tooltip_117");
                    break;
            }
            return result;
        }

        public void GetThumbnail()
        {
            BitmapImage? bitmap = null;
            var filePath = string.Empty;
            var fileService = Utility.GetService<IFileService>();
            var mediaConfigPath = Path.Combine(Constants.OutPath, UserId, Name);
            if (Directory.Exists(mediaConfigPath))
            {
                var config = fileService.ReadFileContent<MediaConfig>(mediaConfigPath, Constants.ConfigFileName, new MediaTypeConverter());
                filePath = config?.Pages.FirstOrDefault()?.ThumbnailFilePath ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, UserId, filePath));
                bitmap.EndInit();
            }

            if (bitmap == null)
            {
                var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/windows-fill.png", UriKind.Absolute);
                var resourceStream = Application.GetResourceStream(uri);
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = resourceStream.Stream;
                bitmap.EndInit();
            }

            Thumbnail = bitmap;
        }
    }
}
