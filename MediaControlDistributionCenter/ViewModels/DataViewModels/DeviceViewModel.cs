using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Helpers.FTP.Server;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaControlDistributionCenter.ViewModels
{
    public partial class DeviceViewModel : DataViewModel<MonitorDto>
    {
        [ObservableProperty]
        private long id;

        [ObservableProperty]
        [Required]
        private string name;

        [ObservableProperty]
        [Required]
        private string sNumber;

        [ObservableProperty]
        private string resolution;

        [ObservableProperty]
        private string lastUpdatedTime;

        [ObservableProperty]
        private int status;

        [ObservableProperty]
        private int enabled;

        [ObservableProperty]
        private string statusText;

        [ObservableProperty]
        private long? groupId;

        [ObservableProperty]
        private string group;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private string mediaNames;

        [ObservableProperty]
        private List<int> mediaIds;

        [ObservableProperty]
        private string ownerName;

        [ObservableProperty]
        [Required]
        private double width;

        [ObservableProperty]
        [Required]
        private double height;

        [ObservableProperty]
        [Required]
        private DateTime startDate;

        [ObservableProperty]
        [Required]
        private DateTime endDate;

        [ObservableProperty]
        [Required]
        private string contactName;

        [ObservableProperty]
        [Required]
        private string contactNumber;

        [ObservableProperty]
        private double? brightness;

        [ObservableProperty]
        private double? volume;

        [ObservableProperty]
        private double? storagePercentage;

        [ObservableProperty]
        private double? usedStoragePercentage;

        [ObservableProperty]
        private string deviceId;

        [ObservableProperty]
        private string enableBtnContent;

        [ObservableProperty]
        private string? sendResult;

        [ObservableProperty]
        private string? uploadResult;

        [ObservableProperty]
        private bool isSendUserCompleted;

        [ObservableProperty]
        private bool isConnected;

        [ObservableProperty]
        private string connectedText;

        [ObservableProperty]
        private ObservableCollection<string> ipAddresses;

        [ObservableProperty]
        private string selectedIpAddress;

        [ObservableProperty]
        private DateTime currentTime;

        [ObservableProperty]
        private BitmapImage? thumbnail;

        private Communication? client;

        public bool IsInternet => client?.IsInternet ?? false;

        public DeviceViewModel()
        {
        }

        public override MonitorDto ToModel()
        {
            return new MonitorDto
            {
                Id = Id,
                Name = Name,
                SnCode = SNumber,
                Status = Status,
                GroupId = GroupId,
                UserAccount = UserId,
                Enabled = Enabled,
                Width = Width,
                Height = Height,
                ValidStart = StartDate.ToString("yyyy-MM-dd"),
                ValidEnd = EndDate.ToString("yyyy-MM-dd"),
                ContactName = ContactName,
                ContactPhone = ContactNumber,
                Brightness = Brightness,
                Volume = Volume,
                StoragePercentage = StoragePercentage,
                DeviceId = SNumber, 
            };
        }

        public override void Binding(MonitorDto model, bool isSelected = false)
        {
            Id = model.Id;
            Name = model.Name;
            SNumber = model.SnCode;
            Resolution = $"{model.Width}*{model.Height}";
            LastUpdatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            Status = model.Status;
            GroupId = model.GroupId;
            UserId = model.UserAccount;
            OwnerName = model.UserName;
            Group = model.MonitorGroupName ?? FindResource("LanguageKey_Code_NoGroup");
            IsSelected = isSelected;
            Enabled = model.Enabled;
            EnableBtnContent = model.Enabled ==  0 ? FindResource("LanguageKey_Code_Enable") : FindResource("LanguageKey_Code_Disable");
            Width = model.Width;
            Height = model.Height;
            StartDate = string.IsNullOrEmpty(model.ValidStart) ? DateTime.Now : DateTime.Parse(model.ValidStart);
            EndDate = string.IsNullOrEmpty(model.ValidEnd) ? DateTime.Now : DateTime.Parse(model.ValidEnd);
            ContactName = model.ContactName;
            ContactNumber = model.ContactPhone;
            Brightness = model.Brightness;
            Volume = model.Volume;
            DeviceId = model.DeviceId;
            StoragePercentage = model.StoragePercentage;
            UsedStoragePercentage = 100 - model.StoragePercentage;
            MediaNames = string.Empty;
            MediaIds = new List<int>();
        }

        public void RefreshStatus() 
        {
            ConnectedText = GetConnectedStatus();
            StatusText = GetStatus();
        }

        public string GetStatus()
        {
            return EndDate < DateTime.Now ? FindResource("LanguageKey_Code_Invalid") : this.Enabled == 0 ? FindResource("LanguageKey_Code_Disable") : Status == 1 ? FindResource("LanguageKey_Code_Online") : FindResource("LanguageKey_Code_Offline");
        }

        public string GetConnectedStatus()
        {
            return IsConnected ? FindResource("LanguageKey_Code_Connected") : FindResource("LanguageKey_Code_Disconnected");
        }

        public void GetPrograms()
        {
            var playbackRecordService = GetService<IPlaybackRecordService>();
            var programService = GetService<IProgramService>();
            var playRecords = playbackRecordService.GetAll(new PlaybackRecordDto { MonitorSnCode = SNumber }).GetAwaiter().GetResult().Data?.ToList() ?? new List<PlaybackRecordDto>();
            foreach (var record in playRecords) 
            {
                var program = programService.GetAll(new ProgramDto { Name = record.MediaName, Status = 1, MediaType = "PROGRAM" }).GetAwaiter().GetResult().Data?.FirstOrDefault();
                if(program != null)
                {
                    MediaNames = program.Name;
                }
            }
        }

        public void GetThumbnail()
        {
            BitmapImage? bitmap = null;
            if (!string.IsNullOrEmpty(MediaNames))
            {
                var programService = GetService<IProgramService>();
                var program = programService.GetAll(new ProgramDto { Name = MediaNames, Status = 1, MediaType = "PROGRAM" }).GetAwaiter().GetResult().Data?.FirstOrDefault();
                if (program != null)
                {
                    var filePath = string.Empty;
                    var fileService = GetService<IFileService>();
                    var mediaConfigPath = Path.Combine(Constants.OutPath, UserId, program.Name);
                    if (Directory.Exists(mediaConfigPath))
                    {
                        var config = fileService.ReadFileContent<MediaConfig>(mediaConfigPath, Constants.ConfigFileName, new MediaTypeConverter());
                        filePath = config?.Pages.FirstOrDefault()?.ThumbnailFilePath ?? string.Empty;
                    }

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.UriSource = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, UserId, filePath));
                        bitmap.EndInit();
                    }
                }
            }

            if (bitmap == null)
            {
                var uri = new Uri($"pack://application:,,,/MediaControlDistributionCenter;component/Assets/windows-fill.png", UriKind.Absolute);
                var resourceStream = Application.GetResourceStream(uri);
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = resourceStream.Stream;
                bitmap.EndInit();
            }

            Thumbnail = bitmap;
        }

        public bool IsRealTimeConnected()
        {
            return client != null && client.netClient.State == Helpers.SocketClient.SocketState.Connected ? true : false;
        }

        [RelayCommand]
        private async Task Connect(Communication client)
        {
            Log.Debug($"Socket status:{client.netClient.State}!");

            this.client = client;
            StatusText = GetStatus();
            IsConnected = true;
            SelectedIpAddress = client.IpAddr;
            ConnectedText = GetConnectedStatus();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void Disconnect()
        {
            if (this.client != null)
            {
                this.client.Disconnect();
                this.client = null;
            }

            StatusText = GetStatus();
            IsConnected = false;
            ConnectedText = GetConnectedStatus();
        }

        [RelayCommand]
        private async Task SendUser()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                IsSendUserCompleted = await interactService.SendUser(this.ToModel(), client);
            }
            catch (Exception ex) 
            {
                ErrorMessage = ex.Message;
                IsSendUserCompleted = false;
            }
        }

        [RelayCommand]
        private async Task VerifyUser(UserViewModel user)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.VerifyUser(this.ToModel(), user.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task VerifySnCode()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.VerifySnCode(this.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task ChangeBrightness(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.ChangeBrightness(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task ChangeVolume(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.ChangeVolume(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task Restart(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.Restart(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task ChangePower(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.ChangePower(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task TimeSync(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.TimeSync(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SyncCurrentTime()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                CurrentTime = await interactService.SyncCurrentTime(this.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SyncBrightness()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                Brightness = await interactService.SyncBrightness(this.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SyncVolume()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                Volume = await interactService.SyncVolume(this.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task TimeGPSSync(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.TimeGPSSync(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task UploadFile(string filePath)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                UploadResult = await interactService.UploadFile(this.ToModel(), filePath, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SendProgram(ProgramViewModel program)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.SendProgram(this.ToModel(), program.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task ChangeProgram(ProgramViewModel program)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.ChangeProgram(this.ToModel(), program.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task EnableMonitor()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.EnableMonitor(this.ToModel(), client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task DeleteProgram(string value)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                await interactService.DeleteProgram(this.ToModel(), value, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SyncFileSync(string fileName)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                SendResult =await interactService.SendSyncFile(this.ToModel(), fileName, client);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SyncDeviceControl(IDeviceControlService deviceControlService)
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                var syncResult = await interactService.SyncDeviceControl(this.ToModel(), client);
                foreach (var item in syncResult)
                {
                    await deviceControlService.Save(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SyncPrograms()
        {
            var interactService = Utility.GetService<IDeviceInteractService>();
            try
            {
                var syncResult = await interactService.SyncPrograms(this.ToModel(), client);
                var programService = GetService<IProgramService>();
                var playRecordService = GetService<IPlaybackRecordService>();
                foreach (var item in syncResult)
                {
                    await programService.Save(item);
                    var model = new PlaybackRecordDto { MediaName = item.Name, MediaType = item.MediaType, MonitorSnCode = SNumber };
                    var existRecord = (await playRecordService.GetAll(model)).Data?.FirstOrDefault();
                    if (existRecord == null)
                    {
                        var response = await playRecordService.Save(model);
                        if (response.Code == 200)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
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
