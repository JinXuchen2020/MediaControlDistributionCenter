using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("DeviceMedias")]
    public class DeviceMedia
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int DeviceId { get; set; }

        [SugarColumn(IsPrimaryKey = true)]
        public int MediaId { get; set; }
    }
}
