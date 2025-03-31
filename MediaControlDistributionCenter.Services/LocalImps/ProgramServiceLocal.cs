using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.Linq.Expressions;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class ProgramServiceLocal : BaseServiceLocal<Program, ProgramDto>, IProgramService
    {
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
