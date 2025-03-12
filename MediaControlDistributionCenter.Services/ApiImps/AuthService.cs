using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class AuthService : Proxy, IAuthService
    {
        public AuthService(string serviceUrl) : base(serviceUrl)
        {
        }

        public async Task<ResultResponse<string>> Login(AccountDto data)
        {
            return await Post<ResultResponse<string>, AccountDto>("/auth/login", data);
        }
    }
}
