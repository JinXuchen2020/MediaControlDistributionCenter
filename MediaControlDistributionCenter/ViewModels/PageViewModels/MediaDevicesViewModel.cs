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
using Serilog;
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

        public MediaDevicesViewModel() 
        {
            this.monitorService = GetService<IMonitorService>();
            this.programService = GetService<IProgramService>();
            this.playbackRecordService = GetService<IPlaybackRecordService>();
            this.publishDevices = new ObservableCollection<DeviceViewModel>();
            RegisterLanguageProperty(this.GetType(), nameof(LoadData));
            //RegisterDevicesChangedAction(this.GetType(), nameof(LoadData));
        }

        public override async Task LoadData()
        {
            var devices = (await monitorService.GetAll(new MonitorDto { UserAccount = CurrentMedia.UserId, Enabled = 1})).Data?.ToList() ?? new List<MonitorDto>();
            var playbackRecords = (await playbackRecordService.GetAll(new PlaybackRecordDto { MediaName = CurrentMedia.Name, MediaType = CurrentMedia.Type })).Data?.ToList() ?? new List<PlaybackRecordDto>();
            var publishedSNCode = playbackRecords.Select(c => c.MonitorSnCode).ToList();
            var devicesList = new List<DeviceViewModel>();
            foreach (var c in devices)
            {
                var viewModel = OnlineDevices.FirstOrDefault(t => t.SnCode == c.SNumber)?.DeviceViewModel;
                if (viewModel == null)
                {
                    viewModel = new DeviceViewModel();
                    viewModel.Binding(c);
                }

                viewModel.IsSelected = viewModel.IsSelected || (viewModel.IsConnected && publishedSNCode.Contains(c.SNumber));
                viewModel.RefreshStatus();
                await viewModel.GetPrograms();
                devicesList.Add(viewModel);
            }

            Devices = new ObservableCollection<DeviceViewModel>(devicesList);
        }

        [RelayCommand]
        private async Task Save()
        {
            var response = await programService.Save(CurrentMedia.ToModel());
            if (response.Code == 200)
            {
            }
        }

        //[RelayCommand]
        //private async Task DetectConnectedDevice()
        //{
        //    await DetectCommunication(CurrentMedia.UserId);
        //    LoadData();
        //}

        [RelayCommand]
        private async Task Publish()
        {
            this.PublishDevices.Clear();
            //await DetectCommunication(CurrentMedia.UserId);
            foreach (var item in Devices)
            {
                var model = new PlaybackRecordDto { MediaName = CurrentMedia.Name, MediaType = CurrentMedia.Type, MonitorSnCode = item.SNumber };
                if (item.IsSelected)
                {
                    await item.PublishProgramCommand.ExecuteAsync(CurrentMedia);
                    Log.Information("发送媒体文件信息到设备成功");

                    if (!string.IsNullOrEmpty(item.SendResult) && item.SendResult == FindResource("LanguageKey_Code_Monitor_Tooltip_120"))
                    {
                        var response = await playbackRecordService.Save(model);
                        if (response.Code == 200)
                        {
                            this.PublishDevices.Add(item);
                            var shelfMedias = (await programService.GetAll(new ProgramDto { Status = 1, MediaType = CurrentMedia.Type })).Data?.ToList() ?? new List<ProgramDto>();
                            foreach (var media in shelfMedias)
                            {
                                media.Status = 0;
                                await programService.Save(media);
                            }
                            await programService.Save(CurrentMedia.ToModel());

                            Log.Information("发送媒体信息到数据库成功");
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
