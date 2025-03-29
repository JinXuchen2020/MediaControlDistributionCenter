using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.ComponentModel.DataAnnotations;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaGroupViewModel : DataViewModel<MediaGroupDto>
    {
        [ObservableProperty]
        [Required]
        public string name;

        [ObservableProperty]
        public long id;

        [ObservableProperty]
        public bool isSelected;

        public bool IsUpdated { get; set; }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }

        public override MediaGroupDto ToModel()
        {
            return new MediaGroupDto
            {
                Id = Id,
                Name = Name
            };
        }

        public override void Binding(MediaGroupDto model, bool selected = false)
        {
            Name = model.Name;
            Id = model.Id;
            IsSelected = selected;
        }
    }
}
