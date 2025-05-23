using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.IO.Pipelines;

namespace MediaControlDistributionCenter.Services
{
    public class DeviceInteractService : IDeviceInteractService
    {
        private readonly IConnectService connectService;
        public DeviceInteractService(IConnectService connectService) 
        {
            this.connectService = connectService;
        }

        public event EventHandler<ProgressEventArgs>? InvokeProgressChanged;

        public async Task<bool> SendUser(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var userInfo = new UsersSync();
            var users = new List<UserSync>();
            var userService = Utility.GetService<IUserService>();
            var adminUser = (await userService.GetAll(new UserDto { Role = "admin" })).Data?.FirstOrDefault();
            if (adminUser != null)
            {
                users.Add(new UserSync(adminUser, null));
            }

            var currentUser = (await userService.GetAll(new UserDto { Account = monitor.UserAccount })).Data?.FirstOrDefault()!;
            if (!string.IsNullOrEmpty(currentUser.AgentAccount))
            {
                var agentUser = (await userService.GetAll(new UserDto { Account = currentUser.AgentAccount })).Data?.FirstOrDefault();
                if (agentUser != null)
                {
                    users.Add(new UserSync(agentUser, null));
                }
            }

            users.Add(new UserSync(currentUser, new MonitorSync(monitor, null)));
            userInfo.Users = users;

            var userInfoString = JsonConvert.SerializeObject(userInfo);
            string path = CommunicationCmd.CmdSendUser + userInfoString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSendUser} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            return true;
        }

        public async Task VerifyUser(MonitorDto monitor, UserDto user, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var userInfo = new { user.Account, user.Password, user.Role };
            var userInfoString = JsonConvert.SerializeObject(userInfo);
            string path = CommunicationCmd.CmdVerifyUser + userInfoString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdVerifyUser} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task VerifySnCode(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdVerifySnCode + monitor.SNumber;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdVerifySnCode} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            //if (client.VerifySnCodeResult == "fail")
            //{
            //    throw new Exception($"{CommunicationCmd.CmdVerifySnCode} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_108")}");
            //}
        }

        public async Task ChangeBrightness(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var modelString = JsonConvert.SerializeObject(deviceControls);
            string path = CommunicationCmd.CmdBrightness + modelString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdBrightness} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var deviceControlService = Utility.GetService<IDeviceControlService>();
            var successFullResult = string.Empty;
            foreach (var deviceControl in deviceControls)
            {
                var response = await deviceControlService.Save(deviceControl);
                if (response.Code == 200)
                {
                    successFullResult = $"{deviceControl.ControlType} {Utility.FindResource("LanguageKey_Code_Control_Tooltip_132")}";
                }
            }

            if (!string.IsNullOrEmpty(successFullResult))
            {
                throw new Exception(successFullResult);
            }
        }

        public async Task ChangeVolume(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var modelString = JsonConvert.SerializeObject(deviceControls);
            string path = CommunicationCmd.CmdVolume + modelString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdVolume} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var deviceControlService = Utility.GetService<IDeviceControlService>();
            var successFullResult = string.Empty;
            foreach (var deviceControl in deviceControls)
            {
                var response = await deviceControlService.Save(deviceControl);
                if (response.Code == 200)
                {
                    successFullResult = $"{deviceControl.ControlType} {Utility.FindResource("LanguageKey_Code_Control_Tooltip_132")}";
                }
            }

            if (!string.IsNullOrEmpty(successFullResult))
            {
                throw new Exception(successFullResult);
            }
        }

        public async Task Restart(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var modelString = JsonConvert.SerializeObject(deviceControls);
            string path = CommunicationCmd.CmdReStart + modelString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdReStart} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var deviceControlService = Utility.GetService<IDeviceControlService>();
            var successFullResult = string.Empty;
            foreach (var deviceControl in deviceControls)
            {
                var response = await deviceControlService.Save(deviceControl);
                if (response.Code == 200)
                {
                    successFullResult = $"{deviceControl.ControlType} {Utility.FindResource("LanguageKey_Code_Control_Tooltip_132")}";
                }
            }

            if (!string.IsNullOrEmpty(successFullResult))
            {
                throw new Exception(successFullResult);
            }
        }

        public async Task ChangePower(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var modelString = JsonConvert.SerializeObject(deviceControls);
            string path = CommunicationCmd.CmdScreen + modelString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdScreen} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var deviceControlService = Utility.GetService<IDeviceControlService>();
            var successFullResult = string.Empty;
            foreach (var deviceControl in deviceControls)
            {
                var response = await deviceControlService.Save(deviceControl);
                if (response.Code == 200)
                {
                    successFullResult = $"{deviceControl.ControlType} {Utility.FindResource("LanguageKey_Code_Control_Tooltip_132")}";
                }
            }

            if (!string.IsNullOrEmpty(successFullResult))
            {
                throw new Exception(successFullResult);
            }
        }

        public async Task TimeSync(MonitorDto monitor, TimeSyncConfigDto model, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var modelString = JsonConvert.SerializeObject(model);
            string path = CommunicationCmd.CmdTime + modelString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdTime} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var timeSyncConfigService = Utility.GetService<ITimeSyncConfigService>();
            var response = await timeSyncConfigService.Save(model);
            if (response.Code == 200)
            {
                throw new Exception($"{Utility.FindResource("LanguageKey_Code_Control_Tooltip_132")}");
            }
        }

        public async Task TimeGPSSync(MonitorDto monitor, TimeSyncConfigDto model, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var modelString = JsonConvert.SerializeObject(model);
            string path = CommunicationCmd.CmdTimeGPS + modelString;
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdTimeGPS} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var timeSyncConfigService = Utility.GetService<ITimeSyncConfigService>();
            var response = await timeSyncConfigService.Save(model);
            if (response.Code == 200)
            {
                throw new Exception($"{Utility.FindResource("LanguageKey_Code_Control_Tooltip_132")}");
            }
        }

        public async Task<string> SendSyncFile(MonitorDto monitor, ProgramDto program, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var sendResult = string.Empty;

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string syncString = JsonConvert.SerializeObject(program, Formatting.Indented);
            string path = CommunicationCmd.CmdSendProgram + syncString;
            var result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                InvokeProgressChanged?.Invoke(this, new ProgressEventArgs(100));
                sendResult = Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_120");
            }
            else
            {
                sendResult = Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_121");
            }

            return sendResult;
        }

        public async Task<string> UploadFile(MonitorDto monitor, string filePath, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var uploadResult = string.Empty;

            var uploadService = Utility.GetService<IUploadService>();
            uploadService.InvokeProgressChanged += InvokeProgressChanged;

            var result = await uploadService.UploadFile(filePath, string.Empty, true);
            if (result.Data)
            {
                InvokeProgressChanged?.Invoke(this, new ProgressEventArgs(100));
                uploadResult = "Successful";// Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_118");
            }
            else
            {
                uploadResult = "Fail"; //Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_119");
            }

            return uploadResult;
        }

        public async Task SendProgram(MonitorDto monitor, ProgramDto program, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string syncString = JsonConvert.SerializeObject(program, Formatting.Indented);
            string path = CommunicationCmd.CmdSendProgram + syncString;
            var result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSendProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task ChangeProgram(MonitorDto monitor, ProgramDto program, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string syncString = JsonConvert.SerializeObject(program, Formatting.Indented);
            string path = CommunicationCmd.CmdChangeProgram + syncString;
            var result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdChangeProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }
        public async Task EnableMonitor(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string syncString = JsonConvert.SerializeObject(monitor, Formatting.Indented);
            string path = CommunicationCmd.CmdEnableMonitor + syncString;
            var result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdEnableMonitor} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task DeleteProgram(MonitorDto monitor, string value, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdDeleteProgram + value;
            var result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdDeleteProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task<DateTime> SyncCurrentTime(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncTime + "Current";
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncTime} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var monitorService = Utility.GetService<IMonitorService>();
            var syncResult = (await monitorService.GetById(monitor.Id)).Data?.CurrentDataTime;
            return string.IsNullOrEmpty(syncResult) ? DateTime.Now : DateTime.Parse(syncResult);
        }

        public async Task SyncBrightness(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncBrightness + "Current";
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncBrightness} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var monitorService = Utility.GetService<IMonitorService>();
            var syncResult = (await monitorService.GetById(monitor.Id)).Data?.Brightness ?? 1;
            monitor.Brightness = syncResult;
        }

        public async Task SyncVolume(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncVolume + "Current";
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncVolume} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            var monitorService = Utility.GetService<IMonitorService>();
            var syncResult = (await monitorService.GetById(monitor.Id)).Data?.Volume ?? 1;
            monitor.Volume = syncResult;
        }

        public async Task SyncDeviceControl(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncDeviceControl + "Control";
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncDeviceControl} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task SyncPrograms(MonitorDto monitor, Communication? client = null)
        {
            if (monitor.ConnectStatus == 0)
            {
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var syncResult = new List<ProgramDto>();

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncProgram + "List";
            bool result = await connectService.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }
    }
}
