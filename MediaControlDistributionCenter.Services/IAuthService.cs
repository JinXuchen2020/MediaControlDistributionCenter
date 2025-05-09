using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IAuthService
    {
        public Task<ResultResponse<TokenDto>> Login(AccountDto data);
    }
}
