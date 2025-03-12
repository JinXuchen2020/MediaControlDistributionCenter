using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services.DTO.Models;
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
        public long id;

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
        public double? size;

        [ObservableProperty]
        public int screensCount;

        [ObservableProperty]
        public string lastUpdatedTime;

        [ObservableProperty]
        public string createdSource;

        [ObservableProperty]
        public int status;

        [ObservableProperty]
        public long? groupId;

        [ObservableProperty]
        public string group;

        [ObservableProperty]
        public string userId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        public string rackingBtnContent;

        public ProgramDto ToModel()
        {
            return new ProgramDto
            {
                Id = Id,
                Name = Name,
                MediaType = Type,
                Resolution = Resolution,
                Size = Size,
                MonitorCount = ScreensCount,
                LastUpdatedTime = LastUpdatedTime,
                CreatedSource = CreatedSource,
                Status = Status,
                GroupId = GroupId,
                UserAccount = UserId,                
            };
        }

        public void Binding(ProgramDto model)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.MediaType;
            Resolution = model.Resolution;
            Width = model.Resolution.Split("*")[0];
            Height = model.Resolution.Split("*")[1];
            Size = model.Size;
            ScreensCount = model.MonitorCount;
            LastUpdatedTime = model.LastUpdatedTime.ToString("yyyy-MM-hh HH:mm");
            CreatedSource = model.CreatedSource;
            Status = model.Status;
            GroupId = model.GroupId;
            UserId = model.UserAccount;
            IsSelected = false;
            Group = model.ProgramGroupName ?? "未分组";
            RackingBtnContent = model.Status == 1 ? "下架" : "上架";
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
