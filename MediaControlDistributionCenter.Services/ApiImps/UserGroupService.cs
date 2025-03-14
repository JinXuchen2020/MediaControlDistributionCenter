using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class UserGroupService : BaseService<UserGroup, UserGroupDto>, IUserGroupService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/userGroup/all"},
            {"GetPageAll", "/userGroup/page"},
            {"GetById", "/userGroup/{0}"},
            {"Save", "/userGroup/save"},
            {"DeleteById", "/userGroup/{0}"},
            {"DeleteBatch", "/userGroup/batch"},
        };

        public UserGroupService(ConnectionMode options) : base(options)
        {
        }
    }
}
