using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("Medias")]
    public class Media
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [Required(ErrorMessage = "名称不能为空")]
        public string Name { get; set; }

        [Required(ErrorMessage = "类型不能为空")]
        public string Type { get; set; }

        [Required(ErrorMessage = "分辨率不能为空")]
        public string Resolution { get; set; }

        [Required(ErrorMessage = "大小不能为空")]
        public string Size { get; set; }

        [Required(ErrorMessage = "大小不能为空")]
        public int ScreensCount { get; set; }

        [Required(ErrorMessage = "大小不能为空")]
        public DateTime LastUpdatedTime { get; set; }

        [Required(ErrorMessage = "大小不能为空")]
        public string CreatedSource { get; set; }

        [Required(ErrorMessage = "大小不能为空")]
        public int Status { get; set; }

        [SugarColumn(IsNullable = true)]
        public int? GroupId { get; set; }
        [Required(ErrorMessage = "用户Id")]
        public int UserId { get; set; }

        [Navigate(typeof(DeviceMedia), nameof(DeviceMedia.MediaId), nameof(DeviceMedia.DeviceId))]
        public IList<Device> Devices { get; set; }

        [SugarColumn(IsIgnore = true)]
        public User User { get; set; }

        [SugarColumn(IsIgnore = true)]
        public MediaGroup? Group { get; set; }

    }
}
