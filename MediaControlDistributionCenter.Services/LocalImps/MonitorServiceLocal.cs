using Azure.Core;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.Linq.Expressions;
using Monitor = MediaControlDistributionCenter.Data.Entity.Monitor;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class MonitorServiceLocal : BaseServiceLocal<Monitor, MonitorDto>, IMonitorService
    {
        public override async Task<ResultResponse<IEnumerable<MonitorDto>>> GetAll(MonitorDto? request)
        {
            Expression result = MakeExpression(request);
            var finalExp = Expression.Lambda<Func<Monitor, bool>>(result, p);
            var results = SQLite.QueryTable<Monitor>()
                    .InnerJoin<User>((c, u) => c.UserAccount == u.Account)
                    .LeftJoin<MonitorGroup>((c, u, dg) => c.GroupId == dg.Id && c.UserAccount == dg.UserAccount)
                    .Where(finalExp)
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
            //IEnumerable<MonitorDto> results;
            //if (request?.UserAccount != null && request?.GroupId != null)
            //{
            //    results = SQLite.QueryTable<Monitor>()
            //        .InnerJoin<User>((d, u) => d.UserAccount == u.Account)
            //        .LeftJoin<MonitorGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserAccount == dg.UserAccount)
            //        .Where((d, u, dg) => d.UserAccount == request.UserAccount && u.AgentAccount == agentAccount && d.GroupId == request.GroupId)
            //        .Select<MonitorDto>()
            //        .ToList();
            //}
            //else if (request?.GroupId != null)
            //{
            //    results = SQLite.QueryTable<Monitor>()
            //        .InnerJoin<User>((d, u) => d.UserAccount == u.Account)
            //        .LeftJoin<MonitorGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserAccount == dg.UserAccount)
            //        .Where((d, u, dg) => u.AgentAccount == agentAccount && d.GroupId == request.GroupId)
            //        .Select<MonitorDto>()
            //        .ToList();
            //}
            //else
            //{
            //    results = SQLite.QueryTable<Monitor>()
            //        .InnerJoin<User>((d, u) => d.UserAccount == u.Account)
            //        .LeftJoin<MonitorGroup>((d, u, dg) => d.GroupId == dg.Id && d.UserAccount == dg.UserAccount)
            //        .Where((d, u, dg) => u.AgentAccount == agentAccount)
            //        .Select<MonitorDto>()
            //        .ToList();
            //}

            Expression result = MakeExpression(request);
            ParameterExpression u = Expression.Parameter(typeof(User), "u");
            var memberInfo = typeof(User).GetMember("AgentAccount").FirstOrDefault();
            var leftExpression = Expression.MakeMemberAccess(u, memberInfo!);
            var rightExpression = Expression.Constant(agentAccount);
            var binaryExp = Expression.Equal(leftExpression, rightExpression);
            result = Expression.AndAlso(result, binaryExp);
            var finalExp = Expression.Lambda<Func<Monitor, User, bool>>(result, p, u);
            var results = SQLite.QueryTable<Monitor>()
                    .InnerJoin<User>((c, u) => c.UserAccount == u.Account)
                    .LeftJoin<MonitorGroup>((c, u, dg) => c.GroupId == dg.Id && c.UserAccount == dg.UserAccount)
                    .Where(finalExp)
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
