using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using System.ComponentModel.DataAnnotations;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceTimeControlViewModel : DataViewModel<DeviceControlDto>
    {
        [ObservableProperty]
        public long id;

        [ObservableProperty]
        public string deviceId;

        [ObservableProperty]
        public string type;

        [ObservableProperty]
        [Required]
        public double? value;

        [ObservableProperty]
        [Required]
        public string executeTime;

        [ObservableProperty]

        public string executeMethod;

        [ObservableProperty]
        [Required]
        public DateTime startDate;

        [ObservableProperty]
        [Required]
        public DateTime endDate;

        [ObservableProperty]
        public string validPeriod;

        [ObservableProperty]
        [Required]
        public string repeatMode;

        [ObservableProperty]
        public string repeatString = string.Empty;

        [ObservableProperty]
        public string userAccount;

        [ObservableProperty]
        public int status;

        [ObservableProperty]
        public string statusText;

        [ObservableProperty]
        public bool isSelected;

        [ObservableProperty]
        public string commandTypeColumnName;

        [ObservableProperty]
        public bool isShow;

        [ObservableProperty]
        public int quarterMonthStart = 1;

        [ObservableProperty]
        public int quarterMonthDayStart = 1;

        [ObservableProperty]
        public int quarterMonthEnd = 1;

        [ObservableProperty]
        public int quarterMonthDayEnd = 1;

        public List<int> MonthDays => new List<int>
        {
            1,2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        };


        public override DeviceControlDto ToModel()
        {
            return new DeviceControlDto
            {
                Id = Id,
                DeviceId = DeviceId,
                ControlType = Type,
                Value = Value,
                Execution = GetExecuteTime(),
                ExecutionType = ExecuteMethod,
                ValidDateStart = StartDate.ToShortDateString(),
                ValidDateEnd = EndDate.ToShortDateString(),
                IsEnabled = Status,
                RepeatMode = RepeatMode,
                UserAccount = UserAccount,
            };
        }

        public override void Binding(DeviceControlDto model, bool isSelected = false)
        {
            DeviceId = model.DeviceId;
            Id = model.Id;
            Type = model.ControlType;
            Value = model.Value;
            ExecuteTime = string.IsNullOrEmpty(model.Execution) ? null : model.Execution.Split("|")[1];
            ExecuteMethod = model.ExecutionType;
            Status = model.IsEnabled;
            StartDate = DateTime.Parse(model.ValidDateStart);
            EndDate = DateTime.Parse(model.ValidDateEnd);
            ValidPeriod = $"{model.ValidDateStart}-{model.ValidDateEnd}";
            StatusText = GetStatus();
            IsSelected = isSelected;
            RepeatMode = model.RepeatMode;
            UserAccount = model.UserAccount;
        }

        public string GetStatus()
        {
            string? result;
            if (DateTime.Now > EndDate)
            {
                result = FindResource("LanguageKey_Code_Expired");
            }
            else if(Status == 1)
            {
                result = FindResource("LanguageKey_Code_Enable");
            }
            else
            {
                result = FindResource("LanguageKey_Code_Disable");
            }
            return result;
        }

        public string GetExecuteTime()
        {
            switch (RepeatMode)
            {
                case "week":
                case "month":
                    return $"{RepeatString}|{ExecuteTime}";
                case "quarter":
                    return $"M{QuarterMonthStart!}D{QuarterMonthDayStart}~M{QuarterMonthEnd}D{QuarterMonthDayEnd}|{ExecuteTime}";
                default:
                    return ExecuteTime;
            }
        }

        public void SetGridColumnName()
        {
            switch (Type)
            {
                case "Brightness":
                    CommandTypeColumnName = FindResource("LanguageKey_Code_Control_Tooltip_119");
                    IsShow = true;
                    break;
                case "Volume":
                    CommandTypeColumnName = FindResource("LanguageKey_Code_Control_Tooltip_120");
                    IsShow = true;
                    break;
                case "TimeSync":
                    CommandTypeColumnName = "";
                    IsShow = false;
                    break;
                case "Restart":
                    CommandTypeColumnName = "";
                    IsShow = false;
                    break;
                default:
                    break;
            }
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }
    }
}
