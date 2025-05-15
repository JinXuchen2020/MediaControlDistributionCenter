using MediaControlDistributionCenter.Helpers.Broadcast;
using MediaControlDistributionCenter.Models;
using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Windows.Threading;

namespace MediaControlDistributionCenter.Services
{

    public class DetectService : IDetectService
    {
        private List<InternetDevice> onlineDevices = [];

        private DispatcherTimer _timer;

        public event EventHandler? InvokeDevicesChanged;

        public async Task StartDetect()
        {
            InitializeTimer();
            await Task.CompletedTask;
        }

        public async Task SendBroadcastMessage()
        {
            await Task.CompletedTask;
        }

        public async Task ConnectDevice(InternetDevice device)
        {
            if (device.Status == 1 && device.DeviceViewModel != null && !device.DeviceViewModel.IsConnected)
            {
                await device.DeviceViewModel.ConnectRemoteCommand.ExecuteAsync(null);
            }
        }

        public IEnumerable<InternetDevice> GetOnlineDevices()
        {
            return [.. onlineDevices];
        }

        private void InitializeTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Task.Run(async () => 
            {
                var monitorService = Utility.GetService<IMonitorService>();
                var devices = (await monitorService.GetAll(new MonitorDto { Status = 1, Enabled = 1 })).Data?.ToList() ?? new List<MonitorDto>();
                var devicesList = new List<InternetDevice>();
                foreach (var c in devices)
                {
                    var device = new InternetDevice
                    {
                        SnCode = c.SnCode,
                        IpAddress = string.Empty,
                        Status = c.Status,
                        IsInternet = true,
                    };

                    device.DeviceViewModel = new DeviceViewModel();
                    device.DeviceViewModel.Binding(c);
                    device.DeviceViewModel.ConnectRemoteCommand.Execute(null);
                }

                onlineDevices = devicesList;
                if(onlineDevices.Count > 0)
                {
                    InvokeDevicesChanged?.Invoke(this, null);
                }
            }).Wait();
        }
    }
}
