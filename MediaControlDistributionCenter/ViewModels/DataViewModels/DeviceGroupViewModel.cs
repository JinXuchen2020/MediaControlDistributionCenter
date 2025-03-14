using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.ComponentModel.DataAnnotations;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceGroupViewModel : DataViewModel<MonitorGroupDto>
    {
        [ObservableProperty]
        [Required]
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

        public override MonitorGroupDto ToModel()
        {
            return new MonitorGroupDto
            {
                Id = Id,
                Name = Name,
                UserAccount = UserId,
            };
        }

        public override void Binding(MonitorGroupDto model, bool selected = false)
        {
            Name = model.Name;
            Id = model.Id;
            UserId = model.UserAccount;
            IsSelected = selected;
        }
    }
}
