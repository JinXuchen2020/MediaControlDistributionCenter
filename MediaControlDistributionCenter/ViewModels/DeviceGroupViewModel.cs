using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Models;
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
        public int id;

        [ObservableProperty]
        public int userId;

        [ObservableProperty]
        public bool isSelected;

        public bool IsUpdated { get; set; }

        public DeviceGroupViewModel()
        {
        }

        public DeviceGroupViewModel(DeviceGroup model, bool selected = false)
        {
            name = model.Name;
            id = model.Id;
            userId = model.UserId;
            isSelected = selected;
        }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }

        public DeviceGroup ToModel()
        {
            return new DeviceGroup
            {
                Id = Id,
                Name = Name,
                UserId = UserId,
            };
        }
    }
}
