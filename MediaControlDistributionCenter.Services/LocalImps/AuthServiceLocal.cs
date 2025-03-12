using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Newtonsoft.Json;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class AuthServiceLocal : IAuthService
    {
        public async Task<ResultResponse<string>> Login(AccountDto data)
        {
            var user = await SQLite.QueryTable<User>()
                .Where(u => u.Account == data.Account && u.Password == data.Password)
                .Select<UserDto>()
                .FirstAsync();
            ResultResponse<string> result;
            if (user != null)
            {
                result = new ResultResponse<string>()
                {
                    Code = 200,
                    Message = "登录成功",
                    Data = JsonConvert.SerializeObject(user),
                };
            }
            else 
            {
                result = new ResultResponse<string>()
                {
                    Code = -1,
                    Message = "登录失败",
                    Data = null,
                };
            }

            return await Task.FromResult(result);
        }
    }
}
