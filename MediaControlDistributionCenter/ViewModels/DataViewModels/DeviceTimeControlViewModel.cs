using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;

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
        public double? value;

        [ObservableProperty]
        public string executeTime;

        [ObservableProperty]
        public string executeMethod;

        [ObservableProperty]
        public DateTime startDate;

        [ObservableProperty]
        public DateTime endDate;

        [ObservableProperty]
        public string validPeriod;

        [ObservableProperty]
        public string repeatMode;

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

        public override DeviceControlDto ToModel()
        {
            return new DeviceControlDto
            {
                Id = Id,
                DeviceId = DeviceId,
                ControlType = Type,
                Value = Value,
                Execution = ExecuteTime,
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
            ExecuteTime = model.Execution;
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
