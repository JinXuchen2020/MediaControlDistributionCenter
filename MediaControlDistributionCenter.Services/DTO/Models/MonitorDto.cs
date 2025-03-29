using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using Monitor = MediaControlDistributionCenter.Data.Entity.Monitor;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class MonitorDto : IMappingProfile<Monitor>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// 显示器名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 显示器SN码
        /// </summary>
        [JsonProperty("snCode", NullValueHandling = NullValueHandling.Ignore)]
        public string SnCode { get; set; }

        /// <summary>
        /// 显示器状态（OFFLINE: 离线, ONLINE: 在线）
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// 显示器启用状态（0: 禁用, 1: 启用）
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public int Enabled { get; set; }

        /// <summary>
        /// 所属用户账号
        /// </summary>
        [JsonProperty("userAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAccount { get; set; }

        /// <summary>
        /// 联系人名字
        /// </summary>
        [JsonProperty("contactName", NullValueHandling = NullValueHandling.Ignore)]
        public string ContactName { get; set; }

        /// <summary>
        /// 联系人电话
        /// </summary>
        [JsonProperty("contactPhone", NullValueHandling = NullValueHandling.Ignore)]
        public string ContactPhone { get; set; }

        /// <summary>
        /// 显示器高度（像素）
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public double Height { get; set; }

        /// <summary>
        /// 显示器宽度（像素）
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public double Width { get; set; }

        /// <summary>
        /// 设备有效期结束
        /// </summary>
        [JsonProperty("validEnd", NullValueHandling = NullValueHandling.Ignore)]
        public string ValidEnd { get; set; }

        /// <summary>
        /// 设备有效期开始
        /// </summary>
        [JsonProperty("validStart", NullValueHandling = NullValueHandling.Ignore)]
        public string ValidStart { get; set; }

        /// <summary>
        /// 亮度（%）
        /// </summary>
        [JsonProperty("brightness", NullValueHandling = NullValueHandling.Ignore)]
        public double? Brightness { get; set; }

        /// <summary>
        /// 显示器音量（%）
        /// </summary>
        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public double? Volume { get; set; }

        /// <summary>
        /// 分组id
        /// </summary>
        [JsonProperty("groupId")]
        public long? GroupId { get; set; }


        [JsonIgnore]
        public string? MonitorGroupName { get; set; }

        [JsonIgnore]
        public string UserName { get; set; }

        /// <summary>
        /// 设备id
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("storagePercentage")]
        public double? StoragePercentage { get; set; }

        ///// <summary>
        ///// 显示器本地账号
        ///// </summary>
        //[JsonProperty("localAccount", NullValueHandling = NullValueHandling.Ignore)]
        //public string LocalAccount { get; set; }

        ///// <summary>
        ///// 显示器本地密码
        ///// </summary>
        //[JsonProperty("localPassword", NullValueHandling = NullValueHandling.Ignore)]
        //public string LocalPassword { get; set; }


        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<Monitor, MonitorDto>();
        }

        public Monitor ToModel()
        {
            return new Monitor
            {
                Id = (int)Id,
                Name = Name,
                SnCode = SnCode,
                Enabled = Enabled,
                UserAccount = UserAccount,
                ContactName = ContactName,
                ContactPhone = ContactPhone,
                Height = Height,
                Width = Width,
                ValidStart = DateTime.Parse(ValidStart),
                ValidEnd = DateTime.Parse(ValidEnd),
                Brightness = Brightness,
                Volume = Volume,
                GroupId = GroupId,
                DeviceId = DeviceId,
                StoragePercentage = StoragePercentage
            };
        }
    }
}
