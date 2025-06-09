using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class ProgramService : BaseServiceLocal<Program, ProgramDto>, IProgramService
    {
        //public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        //{
        //    {"GetAll", "/programme/all"},
        //    {"GetPageAll", "/programme/page"},
        //    {"GetById", "/programme/{0}"},
        //    {"Save", "/programme/save"},
        //    {"DeleteById", "/programme/{0}"},
        //    {"DeleteBatch", "/programme/batch"},
        //};

        //public ProgramService(ConnectionMode options) : base(options)
        //{
        //}

        public override async Task<ResultResponse<IEnumerable<ProgramDto>>> GetAll(ProgramDto? request, bool isSearch = false)
        {
            Expression result = MakeExpression(request, isSearch);

            var finalExp = Expression.Lambda<Func<Program, bool>>(result, p);
            var results = await SQLite.QueryTable<Program>()
                .LeftJoin<ProgramGroup>((c, g) => g.UserAccount == c.UserAccount && g.Id == c.GroupId)
                .Where(finalExp)
                .Select<ProgramDto>()
                .ToListAsync();

            return await Task.FromResult(new ResultResponse<IEnumerable<ProgramDto>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            });
        }
    }
}
