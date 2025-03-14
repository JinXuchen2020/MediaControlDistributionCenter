using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    /// <summary>
    /// 用户
    /// </summary>
    [SugarTable("UserGroups")]
    public class UserGroup : BaseModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string AgentAccount { get; set; }
    }
}
