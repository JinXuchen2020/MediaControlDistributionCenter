using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class ProgramDto : IMappingProfile<Program>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// 媒体名字
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 媒体类型（PROGRAM: 节目, AD: 广告）
        /// </summary>
        [JsonProperty("mediaType", NullValueHandling = NullValueHandling.Ignore)]
        public string MediaType { get; set; }

        /// <summary>
        /// 分辨率
        /// </summary>
        [JsonProperty("resolution", NullValueHandling = NullValueHandling.Ignore)]
        public string Resolution { get; set; }

        /// <summary>
        /// 媒体大小，单位：MB
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public double? Size { get; set; }

        [JsonProperty("monitorCount", NullValueHandling = NullValueHandling.Ignore)]
        public int MonitorCount { get; set; }

        [JsonProperty("lastUpdatedTime", NullValueHandling = NullValueHandling.Ignore)]
        public string LastUpdatedTime { get; set; }

        [JsonProperty("createdSource", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedSource { get; set; }

        /// <summary>
        /// 媒体文件的源地址（目前为本地磁盘地址）
        /// </summary>
        [JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
        public string Src { get; set; }

        /// <summary>
        /// 上下架标识，0-下架，1-上架
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 广告每小时播放次数
        /// </summary>
        [JsonProperty("playCountPerHour")]
        public int? PlayCountPerHour { get; set; }

        [JsonProperty("isHasValidity")]
        public bool IsHasValidity { get; set; }

        [JsonProperty("isHasValidity")]
        public string? ValidStartDate { get; set; }

        [JsonProperty("isHasValidity")]
        public string? ValidEndDate { get; set; }

        /// <summary>
        /// 所属用户账号
        /// </summary>
        [JsonProperty("userAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAccount { get; set; }

        /// <summary>
        /// 所属分组id
        /// </summary>
        [JsonProperty("groupId", NullValueHandling = NullValueHandling.Ignore)]
        public long? GroupId { get; set; }

        [JsonIgnore]
        public string? ProgramGroupName { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<Program, ProgramDto>();
        }

        public Program ToModel()
        {
            return new Program
            {
                Id = (int)Id,
                Name = Name,
                MediaType = MediaType,
                Resolution = Resolution,
                Size = Size,
                MonitorCount = MonitorCount,
                LastUpdatedTime = DateTime.Parse(LastUpdatedTime),
                CreatedSource = CreatedSource,
                Src = Src,
                Status = Status,
                UserAccount = UserAccount,
                GroupId = GroupId,
                PlayCountPerHour = PlayCountPerHour,
                IsHasValidity = IsHasValidity,
                ValidStartDate = string.IsNullOrEmpty(ValidStartDate) ? null : DateTime.Parse(ValidStartDate),
                ValidEndDate = string.IsNullOrEmpty(ValidEndDate) ? null : DateTime.Parse(ValidEndDate),
            };
        }
    }
}
