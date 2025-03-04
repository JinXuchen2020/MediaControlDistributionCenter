using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IUserService
    {
        public Task<UserViewModel?> GetUser(string loginId, string password);

        public Task<IEnumerable<UserViewModel>> GetUsers(int? agentId = null, int? groupId = null);

        public Task<IEnumerable<UserGroupViewModel>> GetUserGroups(int? agentId = null);
    }
}
