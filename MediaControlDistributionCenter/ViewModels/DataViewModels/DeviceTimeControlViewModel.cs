using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Services.DTO.Models;

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
            var result = string.Empty;
            if(DateTime.Now > EndDate)
            {
                result = "过期";
            }
            else if(Status == 1)
            {
                result = "可用";
            }
            else
            {
                result = "禁用";
            }
            return result;
        }

        public void SetGridColumnName()
        {
            switch (Type)
            {
                case "Brightness":
                    CommandTypeColumnName = "亮度值(%)";
                    IsShow = true;
                    break;
                case "Volume":
                    CommandTypeColumnName = "音量值 (%)";
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
    }
}
