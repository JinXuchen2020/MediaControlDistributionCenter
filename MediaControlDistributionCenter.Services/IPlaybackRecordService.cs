using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaControlDistributionCenter.Data.Entity;

namespace MediaControlDistributionCenter.Services
{
    public interface IPlaybackRecordService : IService<PlaybackRecord, PlaybackRecordDto>
    {
    }
}
