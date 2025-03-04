using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("DeviceControls")]
    public class DeviceControl
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public string Type { get; set; }

        public string Value { set; get; }

        public string ExecuteTime { get; set; }

        public string ExecuteMethod { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Status { get; set; }
    }
}
