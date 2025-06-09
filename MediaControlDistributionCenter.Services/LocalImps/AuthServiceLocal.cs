using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class AuthServiceLocal : IAuthService
    {
        public async Task<ResultResponse<TokenDto>> Login(AccountDto data)
        {
            var user = await SQLite.QueryTable<User>()
                .Where(u => u.Account == data.Account && u.Password == data.Password)
                .Select<UserDto>()
                .FirstAsync();
            ResultResponse<TokenDto> result;
            if (user != null)
            {
                result = new ResultResponse<TokenDto>()
                {
                    Code = 200,
                    Message = "登录成功",
                    Data = new TokenDto { Token = JsonConvert.SerializeObject(user) } ,
                };
            }
            else 
            {
                result = new ResultResponse<TokenDto>()
                {
                    Code = -1,
                    Message = "登录失败",
                    Data = null,
                };
            }

            return result;
        }
    }
}
