using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class BaseServiceLocal<Model, DTO> : IService<Model, DTO> where Model : BaseModel, new() where DTO : class, IMappingProfile<Model>
    {
        public virtual async Task<ResultResponse<IEnumerable<DTO>>> GetAll(DTO? request)
        {
            var results = SQLite.QueryTable<Model>()
                    .Select<DTO>()
                    .ToList();

            return await Task.FromResult(new ResultResponse<IEnumerable<DTO>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            });
        }

        public virtual async Task<ResultResponse<IEnumerable<DTO>>> GetPageAll(int pageSize, int page, DTO? request)
        {
            int totalNumber = 0;
            var results = SQLite.QueryTable<Model>()
                    .Select<DTO>().ToPageList(page, pageSize, ref totalNumber)
                    .ToList();

            return await Task.FromResult(new ResultResponse<IEnumerable<DTO>>
            {
                Code = 200,
                Message = "OK",
                Data = results,
                Pagination = new Pagination
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalNumber,
                    TotalPages = (long)Math.Ceiling((decimal)totalNumber / pageSize)
                }
            });
        }

        public virtual async Task<ResultResponse<DTO>> GetById(long id)
        {
            var result = SQLite.QueryTable<Model>()
                .Where(u => u.Id == id)
                .Select<DTO>()
                .First();
            if (result != null)
            {
                return await Task.FromResult(new ResultResponse<DTO>
                {
                    Code = 200,
                    Message = "OK",
                    Data = result
                });
            }
            else
            {
                return await Task.FromResult(new ResultResponse<DTO>
                {
                    Code = -1,
                    Message = "Can't find record",
                    Data = result
                });
            }
        }

        public async Task<ResultResponse<bool>> Save(DTO data)
        {
            var modelData = data.ToModel();
            if (modelData.Id != 0)
            {
                var result = SQLite.UpdateTable(modelData);
                return await Task.FromResult(new ResultResponse<bool>
                {
                    Code = 200,
                    Message = "Ok",
                    Data = result
                });
            }
            else
            {
                var result = SQLite.InserTable(modelData);
                return await Task.FromResult(new ResultResponse<bool>
                {
                    Code = 200,
                    Message = "Ok",
                    Data = result != -1
                });
            }
        }

        public async Task<ResultResponse<bool>> DeleteById(long id)
        {
            var result = SQLite.DeleteById<Model>(id);
            return await Task.FromResult(new ResultResponse<bool>
            {
                Code = 200,
                Message = "Ok",
                Data = result
            });
        }

        public async Task<ResultResponse<bool>> DeleteBatch(IList<long> ids)
        {
            var result = SQLite.DeleteByIds<Model>(ids.Select(c => (object)c).ToList());
            return await Task.FromResult(new ResultResponse<bool>
            {
                Code = 200,
                Message = "Ok",
                Data = result > 0
            });
        }
    }
}
