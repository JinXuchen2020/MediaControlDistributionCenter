using Newtonsoft.Json;
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
    public class User : BaseModel
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// 所属代理商账号
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string AgentAccount { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        [Required]
        public string Company { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Contact { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// 功能字段
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Feature { get; set; }

        /// <summary>
        /// logo地址
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string LogoSrc { get; set; }

        /// <summary>
        /// logo地址
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string LogoFileName { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string Region { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string TimeZone { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string TagLine { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        [Required]
        public string Role { get; set; } = "user";

        /// <summary>
        /// 状态，1表示正常运营，0表示停止运营
        /// </summary>
        [Required]
        public int Status { get; set; }

        /// <summary>
        /// 用户分组id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long? UserGroupId { get; set; }

        [SugarColumn(IsIgnore = true)]
        public UserGroup? Group { get; set; }
    }
}
