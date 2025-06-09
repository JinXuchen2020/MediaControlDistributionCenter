using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("Monitors")]
    public class Monitor : BaseModel
    {
        /// <summary>
        /// 显示器名称
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 显示器SN码
        /// </summary>
        [Required]
        public string SNumber { get; set; }

        /// <summary>
        /// 设备id
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// 显示器启用状态（0: 禁用, 1: 启用）
        /// </summary>
        [Required]
        public int Enabled { get; set; } = 1;

        [Required]
        public int Status { get; set; }

        /// <summary>
        /// 所属用户账号
        /// </summary>
        [Required]
        public string UserAccount { get; set; }

        /// <summary>
        /// 联系人名字
        /// </summary>
        [Required]
        public string ContactName { get; set; }

        /// <summary>
        /// 联系人电话
        /// </summary>
        [Required]
        public string ContactPhone { get; set; }

        /// <summary>
        /// 显示器高度（像素）
        /// </summary>
        [Required]
        public double Height { get; set; }

        /// <summary>
        /// 显示器宽度（像素）
        /// </summary>
        [Required]
        public double Width { get; set; }

        /// <summary>
        /// 设备有效期结束
        /// </summary>
        [Required]
        public DateTime ValidEnd { get; set; }

        /// <summary>
        /// 设备有效期开始
        /// </summary>
        [Required]
        public DateTime ValidStart { get; set; }

        /// <summary>
        /// 亮度（%）
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public double? Brightness { get; set; }

        /// <summary>
        /// 显示器音量（%）
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public double? Volume { get; set; }

        /// <summary>
        /// 分组id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long? GroupId { get; set; }

        [SugarColumn(IsNullable = true)]
        public double? StoragePercentage { get; set; }
    }
}
