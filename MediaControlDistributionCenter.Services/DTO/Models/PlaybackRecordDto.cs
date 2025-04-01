using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class PlaybackRecordDto : BaseDto, IMappingProfile<PlaybackRecord>
    {
        /// <summary>
        /// 当前播放媒体名称
        /// </summary>
        [JsonProperty("mediaName")]
        public string MediaName { get; set; }

        /// <summary>
        /// 媒体类型（节目，广告）
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// 显示器SN码
        /// </summary>
        [JsonProperty("monitorSnCode")]
        public string MonitorSnCode { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<PlaybackRecord, PlaybackRecordDto>();
        }

        public PlaybackRecord ToModel()
        {
            return new PlaybackRecord
            {
                Id = (int)Id,
                MediaName = MediaName,
                MediaType = MediaType,
                MonitorSnCode = MonitorSnCode,
            };
        }
    }
}
