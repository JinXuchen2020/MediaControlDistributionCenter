using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.ComponentModel.DataAnnotations;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class ProgramGroupViewModel : DataViewModel<ProgramGroupDto>
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

        public override ProgramGroupDto ToModel()
        {
            return new ProgramGroupDto
            {
                Id = Id,
                Name = Name,
                UserAccount = UserId,
            };
        }

        public override void Binding(ProgramGroupDto model, bool selected = false)
        {
            Name = model.Name;
            Id = model.Id;
            UserId = model.UserAccount;
            IsSelected = selected;
        }
    }
}
