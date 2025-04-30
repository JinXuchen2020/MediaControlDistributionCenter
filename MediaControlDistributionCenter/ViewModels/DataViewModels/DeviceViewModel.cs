using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Converters;
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
            EndDate = string.IsNullOrEmpty(model.ValidStart) ? DateTime.Now : DateTime.Parse(model.ValidEnd);
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
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            //if (EndDate < DateTime.Now)
            //{
            //    Log.Debug($"Device:{Name} is not valid");
            //    ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
            //    return;
            //}

            var userInfo = new UsersSync();
            var users = new List<UserSync>();
            var userService = GetService<IUserService>();
            var adminUser = userService.GetAll(new UserDto { Role = "admin" }).GetAwaiter().GetResult().Data?.FirstOrDefault();
            if (adminUser != null)
            {
                users.Add(new UserSync(adminUser, null));
            }

            var currentUser = userService.GetAll(new UserDto { Account = UserId }).GetAwaiter().GetResult().Data?.FirstOrDefault()!;
            if (!string.IsNullOrEmpty(currentUser.AgentAccount))
            {
                var agentUser = userService.GetAll(new UserDto { Account = currentUser.AgentAccount }).GetAwaiter().GetResult().Data?.FirstOrDefault();
                if (agentUser != null)
                {
                    users.Add(new UserSync(agentUser, null));
                }
            }

            users.Add(new UserSync(currentUser, new MonitorSync(this.ToModel(), null)));
            userInfo.Users = users;

            var userInfoString = JsonConvert.SerializeObject(userInfo);
            string path = CommunicationCmd.CmdSendUser + userInfoString;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSendUser} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }

            IsSendUserCompleted = true;
        }

        [RelayCommand]
        private async Task VerifyUser(UserViewModel user)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            var userInfo = new { user.Account, user.Password, user.Role };
            var userInfoString = JsonConvert.SerializeObject(userInfo);
            string path = CommunicationCmd.CmdVerifyUser + userInfoString;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdVerifyUser} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task VerifySnCode()
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdVerifySnCode + SNumber;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdVerifySnCode} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }

            if (client.VerifySnCodeResult == "fail")
            {
                ErrorMessage = $"{CommunicationCmd.CmdVerifySnCode} {FindResource("LanguageKey_Code_Device_Tooltip_108")}";
                return;
            }
        }

        [RelayCommand]
        private async Task ChangeBrightness(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdBrightness + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdBrightness} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task ChangeVolume(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdVolume + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdVolume} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task Restart(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdReStart + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdReStart} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task ChangePower(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdScreen + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdScreen} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task TimeSync(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdTime + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdTime} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task SyncCurrentTime()
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                CurrentTime = DateTime.Now;
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdSyncTime + "Current";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSyncTime} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }

            CurrentTime = string.IsNullOrEmpty(client.SyncTimeResult) ? DateTime.Now : DateTime.Parse(client.SyncTimeResult);
        }

        [RelayCommand]
        private async Task SyncBrightness()
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                CurrentTime = DateTime.Now;
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdSyncBrightness + "Current";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSyncBrightness} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }

            Brightness = string.IsNullOrEmpty(client.SyncBrightnessResult) ? 1 : double.Parse(client.SyncBrightnessResult);
        }

        [RelayCommand]
        private async Task SyncVolume()
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                CurrentTime = DateTime.Now;
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdSyncVolume + "Current";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSyncVolume} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }

            Volume = string.IsNullOrEmpty(client.SyncVolumeResult) ? 1 : double.Parse(client.SyncVolumeResult);
        }

        [RelayCommand]
        private async Task TimeGPSSync(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdTimeGPS + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdTimeGPS} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task UploadFile(string filePath)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            client.StartFtpServer();
            var ftpClient = new FtpClient(client.FtpServer);

            var result = await ftpClient.UploadFileToFtpServer(filePath);
            if (result)
            {
                UploadResult = FindResource("LanguageKey_Code_Monitor_Tooltip_118");
            }
            else
            {
                UploadResult = FindResource("LanguageKey_Code_Monitor_Tooltip_119");
            }
        }

        [RelayCommand]
        private async Task SendProgram(ProgramViewModel program)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            var model = program.ToModel();
            string syncString = JsonConvert.SerializeObject(model, Formatting.Indented);
            string path = CommunicationCmd.CmdSendProgram + syncString;
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSendProgram} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task ChangeProgram(ProgramViewModel program)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            var model = program.ToModel();
            string syncString = JsonConvert.SerializeObject(model, Formatting.Indented);
            string path = CommunicationCmd.CmdChangeProgram + syncString;
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdChangeProgram} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task EnableMonitor()
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            var model = this.ToModel();
            string syncString = JsonConvert.SerializeObject(model, Formatting.Indented);
            string path = CommunicationCmd.CmdEnableMonitor + syncString;
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdEnableMonitor} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task DeleteProgram(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdDeleteProgram + value;
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdDeleteProgram} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task SyncFileSync(string fileName)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            var fileSize = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, UserId, fileName)).LongLength;
            var syncObj = new FileSync
            {
                HostName = client.FtpServer._Ip,
                ServerPort = client.FtpServer._port,
                UserName = client.FtpServer._userName,
                Password = client.FtpServer._userPwd,
                FileName = fileName,
                FileSize = fileSize
            };
            string fileSyncString = JsonConvert.SerializeObject(syncObj, Formatting.Indented);
            string path = CommunicationCmd.CmdSyncFile + fileSyncString;
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result) 
            {
                SendResult = FindResource("LanguageKey_Code_Monitor_Tooltip_120");
            }
            else
            {
                SendResult = FindResource("LanguageKey_Code_Monitor_Tooltip_121");
            }
        }

        [RelayCommand]
        private async Task SyncDeviceControl(IDeviceControlService deviceControlService)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdSyncDeviceControl + "Control";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                var syncResult = JsonConvert.DeserializeObject<IList<DeviceControlDto>>(client.SyncDeviceControlResult);
                if (syncResult == null)
                {
                    ErrorMessage = $"{CommunicationCmd.CmdSyncDeviceControl} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                    return;
                }

                foreach (var item in syncResult)
                {
                    await deviceControlService.Save(item);
                }
            }
            else
            {
                ErrorMessage = $"{CommunicationCmd.CmdSyncDeviceControl} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
            }
        }

        [RelayCommand]
        private async Task SyncPrograms()
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (EndDate < DateTime.Now)
            {
                Log.Debug($"Device:{Name} is not valid");
                ErrorMessage = FindResource("LanguageKey_Code_Device_Tooltip_109");
                return;
            }

            string path = CommunicationCmd.CmdSyncProgram + "List";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                var syncResult = JsonConvert.DeserializeObject<IList<ProgramDto>>(client.SyncProgramResult);
                if (syncResult == null)
                {
                    ErrorMessage = $"{CommunicationCmd.CmdSyncProgram} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                    return;
                }

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
            else
            {
                ErrorMessage = $"{CommunicationCmd.CmdSyncProgram} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
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
