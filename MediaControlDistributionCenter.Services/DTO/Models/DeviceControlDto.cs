using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class DeviceControlDto : BaseDto, IMappingProfile<DeviceControl>
    {
        /// <summary>
        /// 控制类型（BRIGHTNESS: 亮度, VOLUME: 音量, RESTART: 重启）
        /// </summary>
        [JsonProperty("controlType")]
        public string ControlType { get; set; }

        /// <summary>
        /// 设备id
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// 执行开始时间，例如：09:00:00（仅适用于定时执行）
        /// </summary>
        [JsonProperty("execution", NullValueHandling = NullValueHandling.Ignore)]
        public string Execution { get; set; }

        /// <summary>
        /// 执行类型（REAL_TIME: 实时, SCHEDULED: 定时）
        /// </summary>
        [JsonProperty("executionType")]
        public string ExecutionType { get; set; }

        /// <summary>
        /// 是否启用，1-启用，0-禁用
        /// </summary>
        [JsonProperty("isEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public int IsEnabled { get; set; }

        /// <summary>
        /// 重复方式，如“每天”或“每周”
        /// </summary>
        [JsonProperty("repeatMode", NullValueHandling = NullValueHandling.Ignore)]
        public string RepeatMode { get; set; }

        /// <summary>
        /// 所属用户账号
        /// </summary>
        [JsonProperty("userAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAccount { get; set; }

        /// <summary>
        /// 结束有效日期，例如：2025-02-01
        /// </summary>
        [JsonProperty("validDateEnd", NullValueHandling = NullValueHandling.Ignore)]
        public string ValidDateEnd { get; set; }

        /// <summary>
        /// 开始有效日期，例如：2025-02-01
        /// </summary>
        [JsonProperty("validDateStart", NullValueHandling = NullValueHandling.Ignore)]
        public string ValidDateStart { get; set; }

        /// <summary>
        /// 控制值，根据控制类型可为亮度值、音量大小等
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public double? Value { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<DeviceControl, DeviceControlDto>();
        }

        public DeviceControl ToModel()
        {
            return new DeviceControl
            {
                Id = (int)Id,
                ControlType = ControlType,
                DeviceId = DeviceId,
                Execution = Execution,
                ExecutionType = ExecutionType,
                IsEnabled = IsEnabled,
                ValidDateStart = DateTime.Parse(ValidDateStart),
                ValidDateEnd = DateTime.Parse(ValidDateEnd),
                Value = Value,
                RepeatMode = RepeatMode,
                UserAccount = UserAccount,
            };
        }
    }
}
