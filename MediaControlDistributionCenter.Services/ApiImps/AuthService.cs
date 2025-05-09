using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class AuthService : Proxy, IAuthService
    {
        public AuthService(ConnectionMode options) : base(options)
        {
        }

        public async Task<ResultResponse<TokenDto>> Login(AccountDto data)
        {
            var result = await Post<ResultResponse<TokenDto>, AccountDto>("/auth/login", data);
            if (result == null)
            {
                result = ResultResponse<TokenDto>.ErrorInstance("Response error");
            }

            return result;
        }
    }
}
