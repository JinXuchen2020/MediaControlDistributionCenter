using MediaControlDistributionCenter.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IDetectService
    {
        event EventHandler InvokeDevicesChanged;

        bool IsStarted { get; }

        public Task StartDetect();

        public void StopDetect();

        public Task SendBroadcastMessage();

        public Task ConnectDevice(InternetDevice device);

        public IEnumerable<InternetDevice> GetOnlineDevices();
    }
}
