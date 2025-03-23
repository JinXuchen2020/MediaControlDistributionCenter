using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaViewModel : DataViewModel<MediaDto>
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]
        [Required]
        private string name;

        [ObservableProperty]
        private string extension;

        [ObservableProperty]
        private string resolution;

        [ObservableProperty]
        private double? size;

        [ObservableProperty]
        private string? src;

        [ObservableProperty]
        private string type;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string? mediaGroupName;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        [Required]
        private double width;

        [ObservableProperty]
        [Required]
        private double height;

        [ObservableProperty]
        private ObservableCollection<MediaGroupViewModel> groups;

        public MediaViewModel()
        {
        }

        public override MediaDto ToModel()
        {
            return new MediaDto
            {
                Id = Id,
                Name = Name,
                GroupId = GroupId,
                Extension = Extension,
                Resolution = $"{Width}*{Height}",
                Size = Size,
                Src = Src,
                Type = Type,
            };
        }

        public override void Binding(MediaDto model, bool isSelected = false)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.Type;
            Resolution = model.Resolution;
            Width = string.IsNullOrEmpty(model.Resolution) ? 0 : double.Parse(model.Resolution.Split("*")[0]);
            Height = string.IsNullOrEmpty(model.Resolution) ? 0 : double.Parse(model.Resolution.Split("*")[1]);
            Size = model.Size;
            GroupId = model.GroupId;
            MediaGroupName = model.MediaGroupName ?? FindResource("LanguageKey_Code_NoGroup");
            Src = model.Src; 
            IsSelected = isSelected;
            Extension = model.Extension;
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }
    }
}
