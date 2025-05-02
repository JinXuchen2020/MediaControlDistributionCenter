using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace MediaControlDistributionCenter.Services
{
    public class DeviceInteractServiceLocal : IDeviceInteractService
    {        
        public async Task<bool> SendUser(MonitorDto monitor, Communication? client = null)
        {
            if (client == null)
            {
                Log.Debug($"Device:{monitor.Name} didn't set client!");
                throw new Exception(Utility.FindResource("LanguageKey_Code_Monitor_Tooltip_116"));
            }

            var userInfo = new UsersSync();
            var users = new List<UserSync>();
            var userService = Utility.GetService<IUserService>();
            var adminUser = userService.GetAll(new UserDto { Role = "admin" }).GetAwaiter().GetResult().Data?.FirstOrDefault();
            if (adminUser != null)
            {
                users.Add(new UserSync(adminUser, null));
            }

            var currentUser = userService.GetAll(new UserDto { Account = monitor.UserAccount }).GetAwaiter().GetResult().Data?.FirstOrDefault()!;
            if (!string.IsNullOrEmpty(currentUser.AgentAccount))
            {
                var agentUser = userService.GetAll(new UserDto { Account = currentUser.AgentAccount }).GetAwaiter().GetResult().Data?.FirstOrDefault();
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
    }
}
