using Azure;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class UserService : BaseService<User, UserDto>
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/user/all"},
            {"GetPageAll", "/user/page"},
            {"GetById", "/user/{0}"},
            {"Save", "/user/save"},
            {"DeleteById", "/user/{0}"},
            {"DeleteBatch", "/user/batch"},
        };

        public UserService(IOptions<ConnectionMode> options) : base(options)
        {
        }
    }
}
