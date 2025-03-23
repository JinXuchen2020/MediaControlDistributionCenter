using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.Linq.Expressions;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class MediaServiceLocal : BaseServiceLocal<Media, MediaDto>, IMediaService
    {
        public override async Task<ResultResponse<IEnumerable<MediaDto>>> GetAll(MediaDto? request)
        {
            Expression result = MakeExpression(request);
            var finalExp = Expression.Lambda<Func<Media, bool>>(result, p);
            var results = SQLite.QueryTable<Media>()
                    .LeftJoin<MediaGroup>((c, dg) => c.GroupId == dg.Id)
                    .Where(finalExp)
                    .Select<MediaDto>()
                    .ToList();

            return await Task.FromResult(new ResultResponse<IEnumerable<MediaDto>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            });
        }
    }
}
