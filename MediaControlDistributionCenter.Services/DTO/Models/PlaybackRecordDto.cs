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
        [JsonProperty("mediaName", NullValueHandling =NullValueHandling.Ignore)]
        public string MediaName { get; set; }

        /// <summary>
        /// 媒体类型（节目，广告）
        /// </summary>
        [JsonProperty("mediaType", NullValueHandling = NullValueHandling.Ignore)]
        public string MediaType { get; set; }

        /// <summary>
        /// 显示器SN码
        /// </summary>
        [JsonProperty("monitorSnCode", NullValueHandling = NullValueHandling.Ignore)]
        public string MonitorSnCode { get; set; }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        [JsonProperty("playSuccess", NullValueHandling = NullValueHandling.Ignore)]
        public bool PlaySuccess { get; set; }

        /// <summary>
        /// 是否定时播放
        /// </summary>
        [JsonProperty("isTimerPlay", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsTimerPlay { get; set; }

        /// <summary>
        /// 下次播放时间
        /// </summary>
        [JsonProperty("playTime", NullValueHandling = NullValueHandling.Ignore)]
        public string? NextPlayTime { get; set; }

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
                PlaySuccess = PlaySuccess,
                IsTimerPlay = IsTimerPlay,
                NextPlayTime = string.IsNullOrEmpty(NextPlayTime) ? null : DateTime.Parse(NextPlayTime),
            };
        }
    }
}
