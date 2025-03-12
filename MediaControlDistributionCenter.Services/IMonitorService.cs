using MediaControlDistributionCenter.Services.DTO.Models;
using Monitor = MediaControlDistributionCenter.Data.Entity.Monitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IMonitorService : IService<Monitor, MonitorDto>
    {
        public Task<ResultResponse<bool>> EnableById(long id, bool isEnable);

        public Task<ResultResponse<IEnumerable<MonitorDto>>> GetAgentAll(string agentAccount, MonitorDto? request);
    }
}
