using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Views;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaViewModel : ObservableValidator
    {
        [ObservableProperty]
        public int id;

        [ObservableProperty]
        [Required(ErrorMessage = "请填写节目名称!")]
        public string name;

        [ObservableProperty]
        [Required(ErrorMessage ="请选择节目类型!")]
        public string type;

        [ObservableProperty]
        public string resolution;

        [ObservableProperty]
        [Required(ErrorMessage = "请填写节目高度!")]
        public string width;

        [ObservableProperty]
        [Required(ErrorMessage = "请填写节目宽度!")]
        public string height;

        [ObservableProperty]
        public string size;

        [ObservableProperty]
        public int screensCount;

        [ObservableProperty]
        public string lastUpdatedTime;

        [ObservableProperty]
        public string createdSource;

        [ObservableProperty]
        public int status;

        [ObservableProperty]
        public int? groupId;

        [ObservableProperty]
        public string group;

        [ObservableProperty]
        public int userId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        public string rackingBtnContent;

        public MediaViewModel()
        {
        }

        public MediaViewModel(Media model)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.Type;
            Resolution = model.Resolution;
            width = model.Resolution?.Split("*")[0];
            height = model.Resolution?.Split("*")[1];
            Size = model.Size;
            ScreensCount = model.ScreensCount;
            lastUpdatedTime = model.LastUpdatedTime.ToString("yyyy-MM-hh HH:mm");
            createdSource = model.CreatedSource;
            status = model.Status;
            groupId = model.Group?.Id;
            userId = model.User.Id;
            isSelected = false;
            group = model.Group?.Name ?? "未分组";
            rackingBtnContent = model.Status == 1 ? "下架" : "上架";
        }

        public Media ToModel()
        {
            return new Media
            {
                Id = Id,
                Name = Name,
                Type = Type,
                Resolution = Resolution,
                Size = Size,
                ScreensCount = ScreensCount,
                LastUpdatedTime = DateTime.Parse(LastUpdatedTime),
                CreatedSource = CreatedSource,
                GroupId = GroupId,
                UserId = UserId,
                Status = Status
            };
        }

        [RelayCommand]
        private void Submit()
        {
            ValidateAllProperties();
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }
    }
}
