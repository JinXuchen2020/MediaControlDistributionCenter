using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monitor = MediaControlDistributionCenter.Data.Entity.Monitor;
using MediaControlDistributionCenter.Data;
using Azure.Core;
using Azure;
using System.Drawing.Printing;
using Microsoft.Extensions.Options;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class MonitorService : BaseService<Monitor, MonitorDto>, IMonitorService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/monitor/all"},
            {"GetPageAll", "/monitor/page"},
            {"GetById", "/monitor/{0}"},
            {"Save", "/monitor/save"},
            {"DeleteById", "/monitor/{0}"},
            {"DeleteBatch", "/monitor/batch"},
            {"Enable", "/monitor/enable"},
        };

        private readonly IUserService userService;

        public MonitorService(ConnectionMode options, IEnumerable<IUserService> userServices) : base(options)
        {
            this.userService = options.Mode == "Local" ? userServices.First(c => c.GetType().Name.EndsWith("Local")) : userServices.First(c => !c.GetType().Name.EndsWith("Local"));
        }

        public async Task<ResultResponse<bool>> EnableById(long id, bool isEnable)
        {
            var parameters = new List<Tuple<string, object>>
            {
                new("id", id),
                new("enable", isEnable ? 1 : 0)
            };

            var queryString = await GetQueryString(parameters);
            var uri = $"{ApiUrls["Enable"]}{queryString}";
            return await Delete<ResultResponse<bool>>(uri);
        }

        public async Task<ResultResponse<IEnumerable<MonitorDto>>> GetAgentAll(string agentAccount, MonitorDto? request)
        {
            var allMonitors = await GetAll(request);
            var agentUsers = await userService.GetAll(new UserDto { AgentAccount = agentAccount });
            var userAccounts = agentUsers.Data?.Select(c => c.Account) ?? new List<string>();
            var results = allMonitors.Data?.Where(c => userAccounts.Contains(c.UserAccount));

            return new ResultResponse<IEnumerable<MonitorDto>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            };
        }
    }
}
