using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaGroupViewModel : ObservableObject
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

        public MediaGroupViewModel()
        {
        }

        public MediaGroupViewModel(MediaGroup mediaGroup, bool selected = false)
        {
            name = mediaGroup.Name;
            id = mediaGroup.Id;
            userId = mediaGroup.UserId;
            isSelected = selected;
        }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }

        public MediaGroup ToModel()
        {
            return new MediaGroup
            {
                Id = Id,
                Name = Name,
                UserId = UserId,
            };
        }
    }
}
