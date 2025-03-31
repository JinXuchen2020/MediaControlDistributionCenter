using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Dm.filter;
using SqlSugar;
using Azure;
using System.Drawing.Printing;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class UserServiceLocal : BaseServiceLocal<User, UserDto>, IUserService
    {
        public override async Task<ResultResponse<IEnumerable<UserDto>>> GetAll(UserDto? request, bool isSearch = false)
        {
            Expression result = MakeExpression(request, isSearch);

            Expression joinUserGroup = Expression.Constant(true);
            ParameterExpression g = Expression.Parameter(typeof(UserGroup), "g");
            if (request == null || string.IsNullOrEmpty(request.Role))
            {
                var memberInfo = typeof(User).GetMember("Role").FirstOrDefault();
                var leftExpression = Expression.MakeMemberAccess(p, memberInfo!);
                var rightExpression = Expression.Constant("admin");
                var binaryExp = Expression.NotEqual(leftExpression, rightExpression);
                result = Expression.AndAlso(result, binaryExp);
            }
            if (request != null && !string.IsNullOrEmpty(request.AgentAccount))
            {
                var memberInfo = typeof(User).GetMember("AgentUserGroupId").FirstOrDefault();
                var leftExpression = Expression.MakeMemberAccess(p, memberInfo!);
                var groupMemberInfo = typeof(UserGroup).GetMember("Id").FirstOrDefault();
                var rightExpression = Expression.MakeMemberAccess(g, groupMemberInfo!);
                var binaryExp = Expression.Equal(Expression.Convert(leftExpression, typeof(long)), Expression.Convert(rightExpression, typeof(long)));

                joinUserGroup = Expression.AndAlso(joinUserGroup, binaryExp);
            }
            else
            {
                var memberInfo = typeof(User).GetMember("AdminUserGroupId").FirstOrDefault();
                var leftExpression = Expression.MakeMemberAccess(p, memberInfo!);
                var groupMemberInfo = typeof(UserGroup).GetMember("Id").FirstOrDefault();
                var rightExpression = Expression.MakeMemberAccess(g, groupMemberInfo!);
                var binaryExp = Expression.Equal(Expression.Convert(leftExpression, typeof(long)), Expression.Convert(rightExpression, typeof(long)));

                joinUserGroup = Expression.AndAlso(joinUserGroup, binaryExp);
            }
            var groupExp = Expression.Lambda<Func<User, UserGroup, bool>>(joinUserGroup, p, g);
            var finalExp = Expression.Lambda<Func<User, bool>>(result, p);
            var results = await SQLite.QueryTable<User>()
                .LeftJoin<UserGroup>(groupExp)
                .Where(finalExp).OrderByDescending(c => c.Role)
                .Select<UserDto>()
                .ToListAsync();

            return await Task.FromResult(new ResultResponse<IEnumerable<UserDto>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            });
        }

        //public override async Task<ResultResponse<IEnumerable<UserDto>>> GetPageAll(int pageSize, int page, UserDto? request)
        //{
        //    int totalNumber = 0;
        //    if (request?.AgentAccount != null)
        //    {
        //        var results = SQLite.QueryTable<User>()
        //            .LeftJoin<UserGroup>((u, g) => g.AgentAccount == u.AgentAccount && g.Id == u.UserGroupId)
        //            .Where(u => u.AgentAccount == request.AgentAccount && (request.UserGroupId == null || u.UserGroupId == request.UserGroupId))
        //            .Select<UserDto>().ToPageList(page, pageSize, ref totalNumber)
        //            .ToList();

        //        return await Task.FromResult(new ResultResponse<IEnumerable<UserDto>>
        //        {
        //            Code = 200,
        //            Message = "OK",
        //            Data = results,
        //            Pagination = new Pagination
        //            {
        //                CurrentPage = page,
        //                PageSize = pageSize,
        //                TotalItems = totalNumber,
        //                TotalPages = (long)Math.Ceiling((decimal)totalNumber / pageSize)
        //            }
        //        });
        //    }
        //    else
        //    {
        //        var results = SQLite.QueryTable<User>()
        //            .LeftJoin<UserGroup>((u, g) => g.AgentAccount == u.AgentAccount && g.Id == u.UserGroupId)
        //            .Where(u => u.Role != "admin" && (request == null || request.UserGroupId == null || u.UserGroupId == request.UserGroupId)).OrderByDescending(u => u.Role)
        //            .Select<UserDto>().ToPageList(page, pageSize, ref totalNumber)
        //            .ToList();

        //        return await Task.FromResult(new ResultResponse<IEnumerable<UserDto>>
        //        {
        //            Code = 200,
        //            Message = "OK",
        //            Data = results,
        //            Pagination = new Pagination
        //            {
        //                CurrentPage = page,
        //                PageSize = pageSize,
        //                TotalItems = totalNumber,
        //                TotalPages = (long)Math.Ceiling((decimal)totalNumber / pageSize)
        //            }
        //        });
        //    }
        //}

        public override async Task<ResultResponse<UserDto>> GetById(long id)
        {
            var result = SQLite.QueryTable<User>()
                .Where(u => u.Id == id)
                .Select<UserDto>()
                .First();
            if(result != null)
            {
                return await Task.FromResult(new ResultResponse<UserDto>
                {
                    Code = 200,
                    Message = "OK",
                    Data = result
                });
            }
            else
            {
                return await Task.FromResult(new ResultResponse<UserDto>
                {
                    Code = -1,
                    Message = "Can't find user",
                    Data = result
                });
            }
        }        
    }
}
