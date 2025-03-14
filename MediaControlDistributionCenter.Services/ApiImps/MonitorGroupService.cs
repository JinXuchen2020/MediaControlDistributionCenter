using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class MonitorGroupService : BaseService<MonitorGroup, MonitorGroupDto>, IMonitorGroupService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/monitorGroup/all"},
            {"GetPageAll", "/monitorGroup/page"},
            {"GetById", "/monitorGroup/{0}"},
            {"Save", "/monitorGroup/save"},
            {"DeleteById", "/monitorGroup/{0}"},
            {"DeleteBatch", "/monitorGroup/batch"},
        };

        public MonitorGroupService(ConnectionMode options) : base(options)
        {
        }
    }
}
