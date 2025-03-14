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
    [SugarTable("Programs")]
    public class Program : BaseModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string MediaType { get; set; }

        [Required]
        public string Resolution { get; set; }

        [SugarColumn(IsNullable = true)]
        public double? Size { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Src { get; set; }

        [Required]
        public int MonitorCount { get; set; }

        [Required]
        public DateTime LastUpdatedTime { get; set; }

        [Required]
        public string CreatedSource { get; set; }

        [Required]
        public int Status { get; set; }

        [SugarColumn(IsNullable = true)]
        public long? GroupId { get; set; }
        
        [Required]
        public string UserAccount { get; set; }

        [SugarColumn(IsIgnore = true)]
        public IList<Monitor> Devices { get; set; }

        [SugarColumn(IsIgnore = true)]
        public User User { get; set; }

        [SugarColumn(IsIgnore = true)]
        public ProgramGroup? Group { get; set; }

    }
}
