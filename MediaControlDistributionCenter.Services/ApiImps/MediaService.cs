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
    public class MediaService : BaseService<Media, MediaDto>, IMediaService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/media/all"},
            {"GetPageAll", "/media/page"},
            {"GetById", "/media/{0}"},
            {"Save", "/media/save"},
            {"DeleteById", "/media/{0}"},
            {"DeleteBatch", "/media/batch"},
        };

        public MediaService(IOptions<ConnectionMode> options) : base(options)
        {
        }
    }
}
