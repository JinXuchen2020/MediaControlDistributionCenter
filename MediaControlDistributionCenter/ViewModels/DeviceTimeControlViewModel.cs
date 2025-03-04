using CommunityToolkit.Mvvm.ComponentModel;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceTimeControlViewModel : ObservableObject
    {
        [ObservableProperty]
        public int id;

        [ObservableProperty]
        public int deviceId;

        [ObservableProperty]
        public string type;

        [ObservableProperty]
        public string value;

        [ObservableProperty]
        public string executeTime;

        [ObservableProperty]
        public string executeMethod;

        [ObservableProperty]
        public DateTime? startDate;

        [ObservableProperty]
        public DateTime? endDate;

        [ObservableProperty]
        public string validPeriod;

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


        public DeviceTimeControlViewModel(DeviceControl deviceControl)
        {
            deviceId = deviceControl.DeviceId;
            id = deviceControl.Id;
            type = deviceControl.Type;
            value = deviceControl.Value;
            executeTime = deviceControl.ExecuteTime;
            executeMethod = deviceControl.ExecuteMethod;
            status = deviceControl.Status;
            startDate = deviceControl.StartDate;
            endDate = deviceControl.EndDate;
            validPeriod = $"{startDate?.ToString("yyyy/MM/dd")}-{endDate?.ToString("yyyy/MM/dd")}";
            statusText = GetStatus();
            isSelected = false;
        }

        public DeviceTimeControlViewModel()
        {
        }

        public DeviceControl ToModel()
        {
            return new DeviceControl
            {
                Id = Id,
                DeviceId = DeviceId,
                Type = Type,
                Value = Value,
                ExecuteTime = ExecuteTime,
                ExecuteMethod = ExecuteMethod,
                StartDate = StartDate!.Value,
                EndDate = EndDate!.Value,
                Status = Status,
            };
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
