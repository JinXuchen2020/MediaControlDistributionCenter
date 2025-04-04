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
        private readonly IProgramService programService;
        private readonly IPlaybackRecordService playbackRecordService;
        private readonly Communication communication;

        public MediaDevicesViewModel(Communication communication) 
        {
            this.monitorService = GetService<IMonitorService>();
            this.programService = GetService<IProgramService>();
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
                result.Binding(c);
                if (ConnectedDevice != null && result.SNumber == ConnectedDevice.SNumber)
                {
                    result.ConnectCommand.Execute(communication);
                    result.IsSelected = publishedSNCode.Contains(c.SnCode);
                }

                result.GetPrograms();
                return result;
            }));
        }

        [RelayCommand]
        private async Task Save()
        {
            var response = await programService.Save(CurrentMedia.ToModel());
            if (response.Code == 200)
            {
            }
        }

        [RelayCommand]
        private async Task DetectConnectedDevice()
        {
            await DetectCommunication(CurrentMedia.UserId);
            LoadData();
        }

        [RelayCommand]
        private async Task Publish()
        {
            this.PublishDevices.Clear();
            foreach (var item in Devices)
            {
                var model = new PlaybackRecordDto { MediaName = CurrentMedia.Name, MediaType = CurrentMedia.Type, MonitorSnCode = item.SNumber };
                if (item.IsSelected)
                {
                    if (ConnectedDevice != null && ConnectedDevice.SNumber == item.SNumber && item.MediaNames == CurrentMedia.Name)
                    {
                        ErrorMessage = FindResource("LanguageKey_Code_Program_Tooltip_118");
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
                            var shelfMedias = (await programService.GetAll(new ProgramDto { Status = 1, MediaType = CurrentMedia.Type })).Data?.ToList() ?? new List<ProgramDto>();
                            foreach (var media in shelfMedias)
                            {
                                media.Status = 2;
                                await programService.Save(media);
                            }
                            CurrentMedia.Status = 1;
                            await programService.Save(CurrentMedia.ToModel());
                        }
                    }
                }
                else
                {
                    if (item.IsConnected)
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
}
