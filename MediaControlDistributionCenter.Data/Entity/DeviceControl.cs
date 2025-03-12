using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("DeviceControls")]
    public class DeviceControl : BaseModel
    {
        public string DeviceId { get; set; }

        public string ControlType { get; set; }

        public double? Value { get; set; }

        public string Execution { get; set; }

        public string ExecutionType { get; set; }

        public string RepeatMode { get; set; }

        public string UserAccount { get; set; }

        public DateTime ValidDateStart { get; set; }

        public DateTime ValidDateEnd { get; set; }

        public int IsEnabled { get; set; }
    }
}
