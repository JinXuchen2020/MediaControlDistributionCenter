using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.Broadcast.Entity
{
    public class UsersSync
    {
        public IList<UserSync> Users { get; set; }
    }

    public class UserSync
    {
        public UserDto User { get; set; }

        public MonitorSync? Monitor { get; set; }

        public UserSync(UserDto user, MonitorSync? monitor)
        {
            this.User = user;
            this.Monitor = monitor;
        }
    }

    public class MonitorSync
    {
        public MonitorDto Monitor { get; set; }

        public IList<ProgramDto> Programs { get; set; }

        public MonitorSync(MonitorDto monitor, IList<ProgramDto>? programs)
        {
            this.Monitor = monitor;
            this.Programs = programs ?? new List<ProgramDto>();
        }
    }
}
