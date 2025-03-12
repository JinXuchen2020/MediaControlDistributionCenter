using Azure.Core;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Monitor = MediaControlDistributionCenter.Data.Entity.Monitor;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class MonitorServiceLocal : BaseServiceLocal<Monitor, MonitorDto>, IMonitorService
    {
        public override async Task<ResultResponse<IEnumerable<MonitorDto>>> GetAll(MonitorDto? request)
        {
            var results = SQLite.QueryTable<Monitor>()
                .InnerJoin<User>((d, u) => d.UserAccount == u.Account)
                .LeftJoin<MonitorGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserAccount == dg.UserAccount)
                .Where((d, u, dg) => (request == null || request.GroupId == null || d.GroupId == request.GroupId) && d.UserAccount == request!.UserAccount)
                .Select<MonitorDto>()
                .ToList();

            return await Task.FromResult(new ResultResponse<IEnumerable<MonitorDto>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            });
        }

        public async Task<ResultResponse<bool>> EnableById(long id, bool isEnable)
        {
            var result = SQLite.QueryTable<Monitor>()
                .Where(u => u.Id == id)
                .First();
            result.Enabled = isEnable ? 1 : 0;

            SQLite.UpdateTable(result);
            return await Task.FromResult(new ResultResponse<bool>
            {
                Code = 200,
                Message = "OK",
                Data = true
            });
        }

        public async Task<ResultResponse<IEnumerable<MonitorDto>>> GetAgentAll(string agentAccount, MonitorDto? request)
        {
            var results = SQLite.QueryTable<Monitor>()
                .InnerJoin<User>((d, u) => d.UserAccount == u.Account)
                .LeftJoin<MonitorGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserAccount == dg.UserAccount)
                .Where((d, u, dg) => u.AgentAccount == agentAccount && (request == null || request.GroupId == null || d.GroupId == request.GroupId) && d.UserAccount == request!.UserAccount)
                .Select<MonitorDto>()
                .ToList();          

            return await Task.FromResult(new ResultResponse<IEnumerable<MonitorDto>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            });
        }
    }
}
