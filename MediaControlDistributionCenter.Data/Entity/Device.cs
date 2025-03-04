using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("Devices")]
    public class Device
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [Required(ErrorMessage = "名称不能为空")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SN号不能为空")]
        public string SNumber { get; set; }

        [Required(ErrorMessage = "分辨率不能为空")]
        public string Resolution { get; set; }

        [Required(ErrorMessage = "分辨率不能为空")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "分辨率不能为空")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "更新时间不能为空")]
        public DateTime LastUpdatedTime { get; set; }

        [Required(ErrorMessage = "分辨率不能为空")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "分辨率不能为空")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "状态不能为空")]
        public int Status { get; set; }

        [SugarColumn(IsNullable = true)]
        public int? GroupId { get; set; }

        [Required(ErrorMessage = "用户Id")]
        public int UserId { get; set; }

        [Navigate(typeof(DeviceMedia), nameof(DeviceMedia.DeviceId), nameof(DeviceMedia.MediaId))]
        public List<Media>? Medias { get; set; }

        [SugarColumn(IsNullable = true)]
        public DeviceGroup? Group { get; set; }

        [SugarColumn(IsNullable = true)]
        public User User { get; set; }
    }
}
