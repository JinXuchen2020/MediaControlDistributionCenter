using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceGroupViewModel : ObservableObject
    {
        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public long id;

        [ObservableProperty]
        public string userId;

        [ObservableProperty]
        public bool isSelected;

        public bool IsUpdated { get; set; }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }

        public MonitorGroupDto ToModel()
        {
            return new MonitorGroupDto
            {
                Id = Id,
                Name = Name,
                UserAccount = UserId,
            };
        }

        public void Binding(MonitorGroupDto model, bool selected = false)
        {
            Name = model.Name;
            Id = model.Id;
            UserId = model.UserAccount;
            IsSelected = selected;
        }
    }
}
