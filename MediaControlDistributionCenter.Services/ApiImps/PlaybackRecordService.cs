using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class PlaybackRecordService : BaseService<PlaybackRecord, PlaybackRecordDto>, IPlaybackRecordService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/playbackRecord/all"},
            {"GetPageAll", "/playbackRecord/page"},
            {"GetById", "/playbackRecord/{0}"},
            {"Save", "/playbackRecord/save"},
            {"DeleteById", "/playbackRecord/{0}"},
            {"DeleteBatch", "/playbackRecord/batch"},
        };

        public PlaybackRecordService(IOptions<ConnectionMode> options) : base(options)
        {
        }
    }
}
