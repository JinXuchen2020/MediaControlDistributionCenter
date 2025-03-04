using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Data.Entity
{
    [SugarTable("UserDetails")]
    public class UserDetail
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? TimeZone { get; set; }

        [SugarColumn(IsNullable = true)]
        public string? Logo { get; set; }

        [SugarColumn(IsNullable = true)]
        public string? CompanyName { get; set; }

        [SugarColumn(IsNullable = true)]
        public string? TagLine { get; set; }
    }
}
