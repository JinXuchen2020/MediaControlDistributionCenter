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
    public class MediaGroupService : BaseService<MediaGroup, MediaGroupDto>, IMediaGroupService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/mediaGroup/all"},
            {"GetPageAll", "/mediaGroup/page"},
            {"GetById", "/mediaGroup/{0}"},
            {"Save", "/mediaGroup/save"},
            {"DeleteById", "/mediaGroup/{0}"},
            {"DeleteBatch", "/mediaGroup/batch"},
        };

        public MediaGroupService(ConnectionMode options) : base(options)
        {
        }
    }
}
