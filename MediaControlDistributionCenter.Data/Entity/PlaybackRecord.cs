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
    [SugarTable("PlaybackRecords")]
    public class PlaybackRecord : BaseModel
    {
        /// <summary>
        /// 当前播放媒体名称
        /// </summary>
        public string MediaName { get; set; }

        /// <summary>
        /// 媒体类型（节目，广告）
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// 显示器SN码
        /// </summary>
        public string MonitorSnCode { get; set; }
    }
}
