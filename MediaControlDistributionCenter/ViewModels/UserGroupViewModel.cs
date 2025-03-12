using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserGroupViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private long id;

        [ObservableProperty]
        private string agentId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private List<UserViewModel> agents;

        public bool IsUpdated { get; set; }

        public UserGroupDto ToModel()
        {
            return new UserGroupDto
            {
                Id = Id,
                Name = Name,
                AgentAccount = AgentId,
            };
        }
        public void Binding(UserGroupDto model, bool isSelected = false)
        {
            Name = model.Name;
            Id = model.Id;
            AgentId = model.AgentAccount;
            IsSelected = isSelected;
        }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }
    }
}
