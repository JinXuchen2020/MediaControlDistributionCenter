using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("MediaGroups")]
    public class MediaGroup
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

        [Required(ErrorMessage = "用户Id")]
        public int UserId { get; set; }
    }
}
