using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using System.Collections.ObjectModel;
using System.IO;

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
        private readonly IPlaybackRecordService playbackRecordService;

        public MediaDevicesViewModel(MediaEditViewModel mediaEditViewModel, MediaManageViewModel mediaManageViewModel) 
        {
            this.monitorService = GetService<IMonitorService>();
            this.playbackRecordService = GetService<IPlaybackRecordService>();
            this.publishDevices = new ObservableCollection<DeviceViewModel>();
            currentMedia = mediaManageViewModel.SelectedMedia ?? mediaEditViewModel.CurrentMedia;
        }

        public override void LoadData(long? groupId = null)
        {
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
                var model = new PlaybackRecordDto { MediaName = CurrentMedia.Name, MediaType = CurrentMedia.Type, MonitorSnCode = item.SNumber };
                if (item.IsSelected)
                {
                    var existRecord = (await playbackRecordService.GetAll(model)).Data?.FirstOrDefault();
                    if (existRecord == null) 
                    {
                        var response = await playbackRecordService.Save(model);
                        if (response.Code == 200)
                        {
                            string filePath = $"{CurrentMedia.Name}.zip";
                            item.UploadFileCommand.Execute(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, filePath));
                            if (!string.IsNullOrEmpty(item.ErrorMessage))
                            {
                                ErrorMessage = item.ErrorMessage;
                                await ShowConfirmDialog();
                                continue;
                            }
                            await item.SendProgramCommand.ExecuteAsync(CurrentMedia);
                            if (!string.IsNullOrEmpty(item.ErrorMessage))
                            {
                                ErrorMessage = item.ErrorMessage;
                                await ShowConfirmDialog();
                                continue;
                            }
                            await item.SyncFileSyncCommand.ExecuteAsync(filePath);
                            if (!string.IsNullOrEmpty(item.ErrorMessage))
                            {
                                ErrorMessage = item.ErrorMessage;
                                await ShowConfirmDialog();
                                continue;
                            }

                            if (!string.IsNullOrEmpty(item.SendResult))
                            {
                                this.PublishDevices.Add(item);
                            }
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
