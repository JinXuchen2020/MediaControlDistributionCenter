using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("MonitorGroups")]
    public class MonitorGroup : BaseModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string UserAccount { get; set; }
    }
}
