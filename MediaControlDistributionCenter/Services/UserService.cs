using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public class UserService : IUserService
    {
        public async Task<UserViewModel?> GetUser(string loginId, string password)
        {
            var result = SQLite.QueryTable<User>()
                .LeftJoin<UserGroup>((u, g) => u.GroupId == g.Id)
                .Where(u => u.Account == loginId && u.Password == password)
                .Select((u, g) => new
                {
                    user = u,
                    group = g
                })
                .First();

            if(result != null)
            {
                result.user.Group = result.group;
                return await Task.FromResult(new UserViewModel(result.user));
            }

            return null;
        }

        public async Task<IEnumerable<UserViewModel>> GetUsers(int? agentId = null, int? groupId = null)
        {
            if (agentId != null)
            {
                var results = SQLite.QueryTable<User>()
                    .LeftJoin<UserGroup>((u, g) => g.AgentId == u.AgentId && g.Id == u.GroupId)
                    .Where(u => u.AgentId == agentId && (groupId == null ? true : u.GroupId == groupId))
                    .Select((u, g)=> new
                    {
                        user = u,
                        group = g
                    })
                    .ToList();

                return await Task.FromResult(results.Select(c => 
                {
                    c.user.Group = c.group;
                    return new UserViewModel(c.user);
                }));
            }
            else
            {
                var results = SQLite.QueryTable<User>()
                    .LeftJoin<UserGroup>((u, g) => g.AgentId == u.AgentId && g.Id == u.GroupId)
                    .Where(u => u.Role != "admin" && (groupId == null ? true : u.GroupId == groupId)).OrderByDescending(u => u.Role)
                    .Select((u, g) => new
                    {
                        user = u,
                        group = g
                    })
                    .ToList();

                return await Task.FromResult(results.Select(c =>
                {
                    c.user.Group = c.group;
                    return new UserViewModel(c.user);
                }));
            }
        }

        public async Task<IEnumerable<UserGroupViewModel>> GetUserGroups(int? agentId = null)
        {
            if (agentId != null)
            {
                var results = SQLite.QueryTable<UserGroup>().Where(d => d.AgentId == agentId).ToList();
                return await Task.FromResult(results.Select(c => new UserGroupViewModel(c)));
            }
            else
            {
                var results = SQLite.QueryTable<UserGroup>().ToList();
                return await Task.FromResult(results.Select(c => new UserGroupViewModel(c)));
            }
        }
    }
}
