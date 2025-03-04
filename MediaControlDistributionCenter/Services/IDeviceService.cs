using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IDeviceService
    {
        public Task<IEnumerable<DeviceViewModel>> GetDevices(int userId, int? groupId = null);

        public Task<IEnumerable<DeviceViewModel>> GetDevices();

        public Task<IEnumerable<DeviceViewModel>> GetAgentDevices(int agentId);

        public Task<IEnumerable<MediaViewModel>> GetMedias(int userId, int? groupId = null);

        public Task<IEnumerable<DeviceTimeControlViewModel>> GetDeviceTimeControls(int deviceId, string type);

        public Task<IEnumerable<DeviceGroupViewModel>> GetDeviceGroups(int? userId = null);

        public Task<IEnumerable<MediaGroupViewModel>> GetMediaGroups(int? userId = null);
    }
}
