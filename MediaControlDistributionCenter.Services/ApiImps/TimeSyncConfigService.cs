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
    public class TimeSyncConfigService : BaseService<TimeSyncConfig, TimeSyncConfigDto>, ITimeSyncConfigService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/timeSyncConfig/all"},
            {"GetPageAll", "/timeSyncConfig/page"},
            {"GetById", "/timeSyncConfig/{0}"},
            {"Save", "/timeSyncConfig/save"},
            {"DeleteById", "/timeSyncConfig/{0}"},
            {"DeleteBatch", "/timeSyncConfig/batch"},
        };

        public TimeSyncConfigService(ConnectionMode options) : base(options)
        {
        }
    }
}
