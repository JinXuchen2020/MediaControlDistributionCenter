using Azure;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class UserService : BaseService<User, UserDto>, IUserService
    {
        private readonly IUserGroupService userGroupService;
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/user/all"},
            {"GetPageAll", "/user/page"},
            {"GetById", "/user/{0}"},
            {"Save", "/user/save"},
            {"DeleteById", "/user/{0}"},
            {"DeleteBatch", "/user/batch"},
        };

        public UserService(IEnumerable<IUserGroupService> userGroupServices, ConnectionMode options) : base(options)
        {
            userGroupService = userGroupServices.First(c => !c.GetType().Name.EndsWith("Local"));
        }

        //public override async Task<ResultResponse<IEnumerable<UserDto>>> GetAll(UserDto? request, bool isSearch = false)
        //{
        //    var parameters = GetQueryByInput(request);

        //    var queryString = await GetQueryString(parameters);

        //    var uri = $"{ApiUrls["GetAll"]}{queryString}";

        //    var result = await GetResponse<ResultResponse<IEnumerable<UserDto>>>(uri.Trim());
        //    if (result == null)
        //    {
        //        result = ResultResponse<IEnumerable<UserDto>>.ErrorInstance("Response error");
        //    }
        //    if (result.Code == 200 && result.Data != null)
        //    {
        //        var resultData = result.Data.ToList();
        //        foreach (UserDto item in resultData)
        //        {
        //            if (request != null && !string.IsNullOrEmpty(request.AgentAccount))
        //            {
        //                if (item.AgentUserGroupId != null)
        //                {
        //                    item.UserGroupName = (await userGroupService.GetById(item.AgentUserGroupId.Value))?.Data?.Name;
        //                }
        //            }
        //            else
        //            {
        //                if (item.AdminUserGroupId != null)
        //                {
        //                    item.UserGroupName = (await userGroupService.GetById(item.AdminUserGroupId.Value))?.Data?.Name;
        //                }
        //            }
        //        }

        //        return new ResultResponse<IEnumerable<UserDto>>
        //        {
        //            Code = 200,
        //            Message = "OK",
        //            Data = resultData
        //        };
        //    }

        //    return result;
        //}
    }
}
