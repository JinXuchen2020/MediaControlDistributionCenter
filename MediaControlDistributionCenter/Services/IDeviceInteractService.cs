using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IDeviceInteractService
    {
        Task<bool> SendUser(MonitorDto monitor, Communication? client = null);
        Task VerifyUser(MonitorDto monitor, UserDto user, Communication? client = null);
        Task VerifySnCode(MonitorDto monitor, Communication? client = null);
        Task ChangeBrightness(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null);
        Task ChangeVolume(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null);
        Task Restart(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null);
        Task ChangePower(MonitorDto monitor, List<DeviceControlDto> deviceControls, Communication? client = null);
        Task TimeSync(MonitorDto monitor, TimeSyncConfigDto model, Communication? client = null);
        Task TimeGPSSync(MonitorDto monitor, TimeSyncConfigDto model, Communication? client = null);
        Task<string> SendSyncFile(MonitorDto monitor, string fileName, Communication? client = null);
        Task<string> UploadFile(MonitorDto monitor, string filePath, Communication? client = null);
        Task SendProgram(MonitorDto monitor, ProgramDto program, Communication? client = null);
        Task ChangeProgram(MonitorDto monitor, ProgramDto program, Communication? client = null);
        Task EnableMonitor(MonitorDto monitor, Communication? client = null);
        Task DeleteProgram(MonitorDto monitor, string value, Communication? client = null);
        Task<DateTime> SyncCurrentTime(MonitorDto monitor, Communication? client = null);
        Task SyncBrightness(MonitorDto monitor, Communication? client = null);
        Task SyncVolume(MonitorDto monitor, Communication? client = null);
        Task SyncDeviceControl(MonitorDto monitor, Communication? client = null);
        Task SyncPrograms(MonitorDto monitor, Communication? client = null);
    }
}
