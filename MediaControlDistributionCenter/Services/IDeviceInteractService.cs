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
    }
}
