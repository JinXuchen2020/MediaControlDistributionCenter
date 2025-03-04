using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    /// <summary>
    /// 用户
    /// </summary>
    [SugarTable("Users")]
    public class User
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 登录账号id
        /// </summary>
        [Required(ErrorMessage = "登录账号")]
        public string Account { get; set; }
         
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "登录账号密码")]
        public string Password { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Region { get; set; } = string.Empty;


        [Required(ErrorMessage = "账号权限")]
        public string Role { get; set; } = "user";

        [SugarColumn(IsNullable = true)]
        public int? GroupId { get; set; }

        [SugarColumn(IsNullable = true)]
        public int? AgentId { get; set; }

        [SugarColumn(IsIgnore = true)]
        public UserGroup? Group { get; set; }
    }
}
