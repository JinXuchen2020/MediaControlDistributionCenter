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
    public class Media : BaseModel
    {
        /// <summary>
        /// 媒体名称
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 媒体文件的后缀，如 jpg, mp4 等
        /// </summary>
        [Required]
        public string Extension { get; set; }

        /// <summary>
        /// 分辨率
        /// </summary>
        [Required]
        public string Resolution { get; set; }

        /// <summary>
        /// 媒体大小（MB）
        /// </summary>
        [Required]
        public double? Size { get; set; }

        /// <summary>
        /// 媒体下载地址
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Src { get; set; }

        /// <summary>
        /// 媒体类型（IMAGE: 图片, VIDEO: 视频）
        /// </summary>
        [Required]
        public string Type { get; set; }
    }
}
