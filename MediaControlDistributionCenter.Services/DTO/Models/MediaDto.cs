using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class MediaDto : IMappingProfile<Media>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// 媒体名称
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// 媒体文件的后缀，如 jpg, mp4 等
        /// </summary>
        [JsonProperty("extension", NullValueHandling = NullValueHandling.Ignore)]
        public string Extension { get; set; }

        /// <summary>
        /// 分辨率
        /// </summary>
        [JsonProperty("resolution", NullValueHandling = NullValueHandling.Ignore)]
        public string Resolution { get; set; }

        /// <summary>
        /// 媒体大小（MB）
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public double? Size { get; set; }

        /// <summary>
        /// 媒体下载地址
        /// </summary>
        [JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
        public string Src { get; set; }

        /// <summary>
        /// 媒体类型（IMAGE: 图片, VIDEO: 视频）
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<Media, MediaDto>();
        }

        public Media ToModel()
        {
            return new Media
            {
                Id = Id,
                Name = Name,
                Extension = Extension,
                Resolution = Resolution,
                Size = Size,
                Src = Src,
                Type = Type,
            };
        }
    }
}
