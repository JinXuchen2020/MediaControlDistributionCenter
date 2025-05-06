using CommunityToolkit.Mvvm.Input;
using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Helpers.Broadcast.Entity;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.IO.Pipelines;

namespace MediaControlDistributionCenter.Services
{
    public class DeviceInteractServiceLocal : IDeviceInteractService
    {        
        public async Task<bool> SendUser(MonitorDto monitor, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
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
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSendUser} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            return true;
        }

        public async Task VerifyUser(MonitorDto monitor, UserDto user, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
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
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdVerifyUser} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task VerifySnCode(MonitorDto monitor, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdVerifySnCode + monitor.SnCode;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdVerifySnCode} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            if (client.VerifySnCodeResult == "fail")
            {
                throw new Exception($"{CommunicationCmd.CmdVerifySnCode} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_108")}");
            }
        }

        public async Task ChangeBrightness(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdBrightness + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdBrightness} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task ChangeVolume(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdVolume + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdVolume} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task Restart(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdReStart + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdReStart} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task ChangePower(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdScreen + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdScreen} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task TimeSync(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdTime + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdTime} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task TimeGPSSync(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdTimeGPS + value;
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdTimeGPS} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task<string> SendSyncFile(MonitorDto monitor, string fileName, Communication? client = null)
        {
            var sendResult = string.Empty;
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            var fileSize = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, monitor.UserAccount, fileName)).LongLength;
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
            var uploadResult = string.Empty;
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            client.StartFtpServer();
            var ftpClient = new FtpClient(client.FtpServer);

            var result = await ftpClient.UploadFileToFtpServer(filePath);
            if (result)
            {
                uploadResult = Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_118");
            }
            else
            {
                uploadResult = Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_119");
            }

            return uploadResult;
        }

        public async Task SendProgram(MonitorDto monitor, ProgramDto program, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
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
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSendProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task ChangeProgram(MonitorDto monitor, ProgramDto program, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
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
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdChangeProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }
        public async Task EnableMonitor(MonitorDto monitor, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
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
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdEnableMonitor} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task DeleteProgram(MonitorDto monitor, string value, Communication? client = null)
        {
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdDeleteProgram + value;
            var result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdDeleteProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
        }

        public async Task<DateTime> SyncCurrentTime(MonitorDto monitor, Communication? client = null)
        {
            var syncResult = DateTime.Now;
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncTime + "Current";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncTime} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            syncResult = string.IsNullOrEmpty(client.SyncTimeResult) ? DateTime.Now : DateTime.Parse(client.SyncTimeResult);
            return syncResult;
        }

        public async Task<double> SyncBrightness(MonitorDto monitor, Communication? client = null)
        {
            double syncResult = 1;
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncBrightness + "Current";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncBrightness} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            syncResult = string.IsNullOrEmpty(client.SyncBrightnessResult) ? 1 : double.Parse(client.SyncBrightnessResult);
            return syncResult;
        }

        public async Task<double> SyncVolume(MonitorDto monitor, Communication? client = null)
        {
            double syncResult = 1;
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncVolume + "Current";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (!result)
            {
                throw new Exception($"{CommunicationCmd.CmdSyncVolume} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            syncResult = string.IsNullOrEmpty(client.SyncVolumeResult) ? 1 : double.Parse(client.SyncVolumeResult);
            return syncResult;
        }

        public async Task<IList<DeviceControlDto>> SyncDeviceControl(MonitorDto monitor, Communication? client = null)
        {
            var syncResult = new List<DeviceControlDto>();
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncDeviceControl + "Control";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                syncResult = JsonConvert.DeserializeObject<List<DeviceControlDto>>(client.SyncDeviceControlResult) ?? throw new Exception($"{CommunicationCmd.CmdSyncDeviceControl} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }
            else
            {
                throw new Exception($"{CommunicationCmd.CmdSyncDeviceControl} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            return syncResult;
        }

        public async Task<IList<ProgramDto>> SyncPrograms(MonitorDto monitor, Communication? client = null)
        {
            var syncResult = new List<ProgramDto>();
            if (client == null)
            {
                Log.Error($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var endDate = string.IsNullOrEmpty(monitor.ValidEnd) ? DateTime.Now : DateTime.Parse(monitor.ValidEnd);
            if (endDate < DateTime.Now)
            {
                Log.Error($"Device:{monitor.Name} is not valid");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Device_Tooltip_109"));
            }

            string path = CommunicationCmd.CmdSyncProgram + "List";
            bool result = await client.ExecuteCmdAsync(path, TimeSpan.FromMilliseconds(3000));
            if (result)
            {
                syncResult = JsonConvert.DeserializeObject<List<ProgramDto>>(client.SyncProgramResult);
                if (syncResult == null)
                {
                    throw new Exception($"{CommunicationCmd.CmdSyncProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
                }
            }
            else
            {
                throw new Exception($"{CommunicationCmd.CmdSyncProgram} {Utility.FindResource("LanguageKey_Code_Device_Tooltip_101")}");
            }

            return syncResult;
        }
    }
}
