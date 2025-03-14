using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services.DTO.Models;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class UserGroupViewModel : DataViewModel<UserGroupDto>
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

        public override UserGroupDto ToModel()
        {
            return new UserGroupDto
            {
                Id = Id,
                Name = Name,
                AgentAccount = AgentId,
            };
        }
        public override void Binding(UserGroupDto model, bool isSelected = false)
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
