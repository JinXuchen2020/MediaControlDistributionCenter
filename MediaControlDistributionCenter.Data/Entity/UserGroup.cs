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
    public class UserGroup
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "名称不能为空")]
        public string Name { get; set; }

        [Required(ErrorMessage = "代理商Id")]
        public int AgentId { get; set; }
    }
}
