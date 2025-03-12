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
    [SugarTable("TimeSyncConfigs")]
    public class TimeSyncConfig : BaseModel
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 对时模式（手动, NTP对时, 射频对时, GPS对时）
        /// </summary>
        public string SyncMode { get; set; }

        /// <summary>
        /// 时区，例如：UTC+08:00
        /// </summary>
        public string Timezone { get; set; }

        /// <summary>
        /// 所属用户账号
        /// </summary>
        public string UserAccount { get; set; }
    }
}
