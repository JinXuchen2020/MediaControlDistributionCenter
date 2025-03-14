using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class DeviceControlService : BaseService<DeviceControl, DeviceControlDto>, IDeviceControlService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/deviceControl/all"},
            {"GetPageAll", "/deviceControl/page"},
            {"GetById", "/deviceControl/{0}"},
            {"Save", "/deviceControl/save"},
            {"DeleteById", "/deviceControl/{0}"},
            {"DeleteBatch", "/deviceControl/batch"},
        };

        public DeviceControlService(ConnectionMode options) : base(options)
        {
        }
    }
}
