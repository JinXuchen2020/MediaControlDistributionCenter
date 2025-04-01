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
    public class TimeSyncConfigDto : BaseDto, IMappingProfile<TimeSyncConfig>
    {
        /// <summary>
        /// 设备id
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// 对时模式（手动, NTP对时, 射频对时, GPS对时）
        /// </summary>
        [JsonProperty("syncMode", NullValueHandling = NullValueHandling.Ignore)]
        public string SyncMode { get; set; }

        /// <summary>
        /// 时区，例如：UTC+08:00
        /// </summary>
        [JsonProperty("timezone", NullValueHandling = NullValueHandling.Ignore)]
        public string Timezone { get; set; }

        [JsonProperty("currentDate", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentDate { get; set; }

        /// <summary>
        /// 所属用户账号
        /// </summary>
        [JsonProperty("userAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAccount { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<TimeSyncConfig, TimeSyncConfigDto> ();
        }

        public TimeSyncConfig ToModel()
        {
            return new TimeSyncConfig
            {
                Id = (int)Id,
                DeviceId = DeviceId,
                SyncMode = SyncMode,
                Timezone = Timezone,
                UserAccount = UserAccount
            };
        }
    }
}
