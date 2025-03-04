using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserGroupViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private int agentId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private List<UserViewModel> agents;

        public bool IsUpdated { get; set; }

        public UserGroupViewModel()
        {
        }

        public UserGroupViewModel(UserGroup model, bool selected = false)
        {
            name = model.Name;
            id = model.Id;
            agentId = model.AgentId;
            isSelected = selected;
        }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }

        public UserGroup ToModel()
        {
            return new UserGroup
            {
                Id = Id,
                Name = Name,
                AgentId = AgentId,
            };
        }
    }
}
