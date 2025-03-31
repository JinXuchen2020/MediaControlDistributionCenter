using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Helpers.Tool;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;

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
        private string startDate;

        [ObservableProperty]
        [Required]
        private string endDate;

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
        private string deviceId;

        [ObservableProperty]
        private string enableBtnContent;

        [ObservableProperty]
        private string? sendResult;

        [ObservableProperty]
        private string? uploadResult;

        private Communication? client;

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
                Status = StatusText,
                GroupId = GroupId,
                UserAccount = UserId,
                Enabled = Enabled,
                Width = Width,
                Height = Height,
                ValidStart = StartDate,
                ValidEnd = EndDate,
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
            Status = model.Status == "ONLINE" ? 1 : 0;
            GroupId = model.GroupId;
            UserId = model.UserAccount;
            OwnerName = model.UserName;
            Group = model.MonitorGroupName ?? FindResource("LanguageKey_Code_NoGroup");
            IsSelected = isSelected;
            Enabled = model.Enabled;
            StatusText = GetStatus();
            EnableBtnContent = model.Enabled ==  0 ? FindResource("LanguageKey_Code_Enable") : FindResource("LanguageKey_Code_Disable");
            Width = model.Width;
            Height = model.Height;
            StartDate = model.ValidStart;
            EndDate = model.ValidEnd;
            ContactName = model.ContactName;
            ContactNumber = model.ContactPhone;
            Brightness = model.Brightness;
            Volume = model.Volume;
            DeviceId = model.DeviceId;
            StoragePercentage = model.StoragePercentage;
            MediaNames = string.Empty;
            MediaIds = new List<int>();
        }

        public string GetStatus()
        {
            return this.Enabled == 0 ? FindResource("LanguageKey_Code_Disable") : client != null && client.netClient.State == Helpers.SocketClient.SocketState.Connected ? FindResource("LanguageKey_Code_Online") : FindResource("LanguageKey_Code_Offline");
        }

        public bool IsConnected()
        {
            return client != null && client.netClient.State == Helpers.SocketClient.SocketState.Connected ? true : false;
        }

        [RelayCommand]
        private async Task Connect(Communication client)
        {
            Log.Debug($"Socket status:{client.netClient.State}!");
            if (ConnectionMode.Mode == "Local" && client.netClient.State == Helpers.SocketClient.SocketState.Connected)
            {
                Log.Debug($"Device:{Name} has connected!");
                this.client = client;
                StatusText = GetStatus();
                Log.Debug($"Device:{Name} setting comopleted!");
                return;
            }

            var ipAddress = NetworkTool.GetGatewayIp();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_117");
                return;
            }

            if (ConnectionMode.Mode == "Remote" && client.netClient.State == Helpers.SocketClient.SocketState.Connected && ipAddress != client.IpAddr)
            {
               client.Disconnect();
            }

            client.Connect(ipAddress, "5001");
            int count = 1;
            while (client.netClient.State != Helpers.SocketClient.SocketState.Connected && count > 0)
            {
                Thread.Sleep(500);
                count--;
            }

            if (client.netClient.State != Helpers.SocketClient.SocketState.Connected)
            {
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_117");
                return;
            }

            this.client = client;
            StatusText = GetStatus();
            Log.Debug($"Device:{Name} connected success!");
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
        }

        [RelayCommand]
        private async Task SendUser(UsersSync users)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
            }

            var userInfo = users;
            var userInfoString = JsonConvert.SerializeObject(userInfo);
            string path = CommunicationCmd.CmdSendUser + userInfoString;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdSendUser} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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
        private async Task ChangeBrightness(string value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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
        private async Task TimeSync(DateTime value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
            }

            string path = CommunicationCmd.CmdTime + value.ToString();
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                ErrorMessage = $"{CommunicationCmd.CmdTime} {FindResource("LanguageKey_Code_Device_Tooltip_101")}";
                return;
            }
        }

        [RelayCommand]
        private async Task TimeGPSSync(DateTime value)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
            }

            string path = CommunicationCmd.CmdTimeGPS + value.ToString();
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
            var ftpClient = App.ServicesProvider.GetRequiredService<FtpClient>();

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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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
        private async Task DeleteProgram(ProgramViewModel program)
        {
            if (client == null)
            {
                Log.Debug($"Device:{Name} didn't set client!");
                ErrorMessage = FindResource("LanguageKey_Code_Monitor_Tooltip_116");
                return;
            }

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
            }

            var model = program.ToModel();
            string syncString = JsonConvert.SerializeObject(model, Formatting.Indented);
            string path = CommunicationCmd.CmdDeleteProgram + syncString;
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
            }

            var fileSize = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, UserId, fileName)).LongLength;
            var syncObj = new FileSync
            {
                HostName = client.Heart.FtpIp,
                ServerPort = int.Parse(client.Heart.FtpPort),
                UserName = client.Heart.FtpUserName,
                Password = client.Heart.FtpUserPwd,
                FileName = fileName,
                FileSize = fileSize
            };
            string fielSyncString = JsonConvert.SerializeObject(syncObj, Formatting.Indented);
            string path = CommunicationCmd.CmdSyncFile + fielSyncString;
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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

            if (!IsConnected())
            {
                Log.Debug($"Device:{Name} need to connected again!");
                await Connect(client);
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
