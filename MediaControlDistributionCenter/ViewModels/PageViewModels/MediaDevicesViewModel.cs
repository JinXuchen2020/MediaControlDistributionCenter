using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
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
        private ProgramViewModel currentMedia;

        [ObservableProperty]
        private bool isCover;

        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> publishDevices;

        private readonly IMonitorService monitorService;
        private readonly IPlaybackRecordService playbackRecordService;
        private readonly Communication communication;

        public MediaDevicesViewModel(Communication communication) 
        {
            this.monitorService = GetService<IMonitorService>();
            this.playbackRecordService = GetService<IPlaybackRecordService>();
            this.publishDevices = new ObservableCollection<DeviceViewModel>();
            this.communication = communication;
        }

        public override void LoadData()
        {
            var devices = monitorService.GetAll(new MonitorDto { UserAccount = CurrentMedia.UserId, Enabled = 1}).GetAwaiter().GetResult().Data?.ToList() ?? new List<MonitorDto>();
            var playbackRecords = playbackRecordService.GetAll(new PlaybackRecordDto { MediaName = CurrentMedia.Name, MediaType = CurrentMedia.Type }).GetAwaiter().GetResult().Data?.ToList() ?? new List<PlaybackRecordDto>();
            var publishedSNCode = playbackRecords.Select(c=>c.MonitorSnCode).ToList();
            Devices = new ObservableCollection<DeviceViewModel>(devices.Select(c =>
            {
                var result = new DeviceViewModel();
                result.Binding(c, publishedSNCode.Contains(c.SnCode));
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
                    if (existRecord == null || IsCover) 
                    {
                        item.ConnectCommand.Execute(communication);
                        if (!string.IsNullOrEmpty(item.ErrorMessage))
                        {
                            ErrorMessage = item.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            continue;
                        }
                        string filePath = $"{CurrentMedia.Name}.zip";
                        item.UploadFileCommand.Execute(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, item.UserId, filePath));
                        if (!string.IsNullOrEmpty(item.ErrorMessage))
                        {
                            ErrorMessage = item.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            continue;
                        }
                        await item.SendProgramCommand.ExecuteAsync(CurrentMedia);
                        if (!string.IsNullOrEmpty(item.ErrorMessage))
                        {
                            ErrorMessage = item.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            continue;
                        }
                        await item.SyncFileSyncCommand.ExecuteAsync(filePath);
                        if (!string.IsNullOrEmpty(item.ErrorMessage))
                        {
                            ErrorMessage = item.ErrorMessage;
                            await ShowConfirmDialogCommand.ExecuteAsync(null);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(item.SendResult) && item.SendResult == FindResource("LanguageKey_Code_Monitor_Tooltip_120"))
                        {
                            var response = await playbackRecordService.Save(model);
                            if (response.Code == 200)
                            {
                                this.PublishDevices.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    var existRecord = (await playbackRecordService.GetAll(model)).Data?.FirstOrDefault();
                    if (existRecord != null)
                    {
                        await playbackRecordService.DeleteById(existRecord.Id);
                    }
                }
            }
        }
    }
}
