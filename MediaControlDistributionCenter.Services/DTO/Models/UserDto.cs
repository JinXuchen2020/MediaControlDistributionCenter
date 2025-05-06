using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    /// <summary>
    /// UserDTO
    /// </summary>
    public class UserDto : BaseDto, IMappingProfile<User>
    {
        /// <summary>
        /// 账号
        /// </summary>
        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public string Account { get; set; }

        /// <summary>
        /// 所属代理商账号
        /// </summary>
        [JsonProperty("agentAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string? AgentAccount { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        [JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
        public string Company { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        [JsonProperty("contact", NullValueHandling = NullValueHandling.Ignore)]
        public string Contact { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        /// <summary>
        /// 功能字段
        /// </summary>
        [JsonProperty("feature", NullValueHandling = NullValueHandling.Ignore)]
        public string Feature { get; set; }

        /// <summary>
        /// logo地址
        /// </summary>
        [JsonProperty("logoFileName", NullValueHandling = NullValueHandling.Ignore)]
        public string? LogoFileName { get; set; }

        /// <summary>
        /// logo地址
        /// </summary>
        [JsonProperty("logoSrc", NullValueHandling = NullValueHandling.Ignore)]
        public string? LogoSrc { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        [JsonProperty("tagLine", NullValueHandling = NullValueHandling.Ignore)]
        public string? TagLine { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string? TimeZone { get; set; }        

        /// <summary>
        /// 权限
        /// </summary>
        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
        public string Role { get; set; }

        /// <summary>
        /// 状态，1表示正常运营，0表示停止运营
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }

        /// <summary>
        /// 用户分组id
        /// </summary>
        [JsonProperty("adminGroupId", NullValueHandling = NullValueHandling.Ignore)]
        public long? AdminUserGroupId { get; set; }

        /// <summary>
        /// 用户分组id
        /// </summary>
        [JsonProperty("userGroupId", NullValueHandling = NullValueHandling.Ignore)]
        public long? AgentUserGroupId { get; set; }

        [JsonIgnore]
        public string? UserGroupName { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<User, UserDto>();
        }

        public User ToModel()
        {
            return new User
            {
                Id = (int)Id,
                Email = Email,
                Account = Account,
                Password = Password,
                Role = Role,
                Status = Status,
                AdminUserGroupId = AdminUserGroupId,
                AgentUserGroupId = AgentUserGroupId,
                Region = Region,
                Company = Company,
                Contact = Contact,
                AgentAccount = AgentAccount,
                Feature = Feature,
                LogoSrc = LogoSrc,
                LogoFileName = LogoFileName,
                TimeZone= TimeZone,
                TagLine = TagLine
            };
        }
    }
}
