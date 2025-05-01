using MediaControlDistributionCenter.Models;

namespace MediaControlDistributionCenter.Services
{

    public class DetectService : IDetectService
    {
        private readonly List<InternetDevice> onlineDevices = [];

        public event EventHandler? InvokeDevicesChanged;

        public async Task StartDetect()
        {
            await Task.CompletedTask;
        }

        public async Task SendBroadcastMessage()
        {
            await Task.CompletedTask;
        }

        public async Task ConnectDevice(InternetDevice device)
        {
            await Task.CompletedTask;
        }

        public IEnumerable<InternetDevice> GetOnlineDevices()
        {
            return [.. onlineDevices];
        }
    }
}
