using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class MediaDevicesViewModel : PageViewModel
    {
        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> devices;

        [ObservableProperty]
        private MediaViewModel currentMedia;

        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> publishDevices;

        private readonly IMonitorService monitorService;

        public MediaDevicesViewModel(IMonitorService monitorService) 
        {
            this.monitorService = monitorService;
            this.publishDevices = new ObservableCollection<DeviceViewModel>();
        }

        public void SetValues(MediaViewModel mediaViewModel)
        {
            CurrentMedia = mediaViewModel;

            var devices = monitorService.GetAll(null).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
            Devices = new ObservableCollection<DeviceViewModel>(devices.Select(c => 
            {
                var result = new DeviceViewModel();
                result.Binding(c);
                return result;
            }));
        }

        [RelayCommand]
        private async Task Publish()
        {
            foreach (var item in Devices)
            {
                var model = new DeviceMedia { DeviceId = item.Id, MediaId = CurrentMedia.Id };
                if (item.IsSelected)
                {
                    if (SQLite.QueryTable<DeviceMedia>().Where(c => c.DeviceId == item.Id && c.MediaId == CurrentMedia.Id).First() == null) 
                    {
                        SQLite.InserTable(model);

                        string filePath = $"{CurrentMedia.Name}.zip";
                        item.UploadFileCommand.Execute(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, filePath));
                        await item.SyncFileSyncCommand.ExecuteAsync(filePath);

                        if (!string.IsNullOrEmpty(item.SendResult))
                        {
                            this.PublishDevices.Add(item);
                        }
                    }
                }
                else
                {
                    SQLite.DeleTeable(model);
                }                
            }
        }

        [RelayCommand]
        private async Task ShowConfirmDialog()
        {
            var dialog = new ResultConfirmDialog(this);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, Constants.DialogHostId);
        }
    }
}
