using Azure.Core;
using MediaControlDistributionCenter.Data;
using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class BaseServiceLocal<Model, DTO> : IService<Model, DTO> where Model : BaseModel, new() where DTO : class, IMappingProfile<Model>
    {
        protected ParameterExpression p = Expression.Parameter(typeof(Model), "c");

        public virtual async Task<ResultResponse<IEnumerable<DTO>>> GetAll(DTO? request)
        {
            var expression = MakeExpression(request);
            var results = await SQLite.QueryTable<Model>()
                    .Where(Expression.Lambda<Func<Model, bool>>(expression, p))
                    .Select<DTO>()
                    .ToListAsync();

            return new ResultResponse<IEnumerable<DTO>>
            {
                Code = 200,
                Message = "OK",
                Data = results
            };
        }

        public virtual async Task<ResultResponse<IEnumerable<DTO>>> GetPageAll(int pageSize, int page, DTO? request)
        {
            int totalNumber = 0;
            var expression = MakeExpression(request);
            var results = SQLite.QueryTable<Model>()
                    .Where(Expression.Lambda<Func<Model, bool>>(expression, p))
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

        public virtual async Task<ResultResponse<bool>> Save(DTO data)
        {
            var modelData = data.ToModel();
            if (modelData.Id != 0 && SQLite.QueryTable<Model>().First(c => c.Id == modelData.Id) != null)
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

        protected virtual Expression MakeExpression(DTO? request)
        {
            Expression result = Expression.Constant(true);
            if (request != null)
            {
                var properties = request.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(request);

                    if (value != null && !value.Equals(DefaultForType(property.PropertyType)))
                    {
                        var memberInfo = typeof(Model).GetMember(property.Name).FirstOrDefault() as PropertyInfo;
                        if (memberInfo != null)
                        {
                            var leftExpression = Expression.MakeMemberAccess(p, memberInfo);
                            var rightExpression = Expression.Convert(Expression.Constant(value), memberInfo.PropertyType);
                            var binaryExp = Expression.Equal(leftExpression, rightExpression);
                            result = Expression.AndAlso(result, binaryExp);
                        }
                    }
                }
            }

            return result;
        }

        public object? DefaultForType(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;  
        }
    }
}
