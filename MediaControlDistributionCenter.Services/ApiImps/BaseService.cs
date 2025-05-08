using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class BaseService<Model, DTO> : Proxy, IService<Model, DTO> where Model : BaseModel, new() where DTO : class, IMappingProfile<Model> 
    {
        public virtual Dictionary<string, string> ApiUrls { get; set; }

        public BaseService(ConnectionMode options) : base(options)
        {
        }

        public virtual async Task<ResultResponse<IEnumerable<DTO>>> GetAll(DTO? request, bool isSearch = false)
        {
            var parameters = GetQueryByInput(request);
            var queryString = await GetQueryString(parameters);

            var uri = $"{ApiUrls["GetAll"]}{queryString}";
            var result = await GetResponse<ResultResponse<IEnumerable<DTO>>>(uri.Trim());
            if (result == null)
            {
                result = ResultResponse<IEnumerable<DTO>>.ErrorInstance("Repsonse error");
            }
            return result;
        }

        public async Task<ResultResponse<IEnumerable<DTO>>> GetPageAll(int pageSize, int page, DTO? request)
        {
            var parameters = new List<Tuple<string, object>>
            {
                new("page", page),
                new("pageSize", pageSize)
            };

            parameters.AddRange(GetQueryByInput(request));
            var queryString = await GetQueryString(parameters);

            var uri = $"{ApiUrls["GetPageAll"]}{queryString}";
            var result = await GetResponse<ResultResponse<IEnumerable<DTO>>>(uri);
            if (result == null)
            {
                result = ResultResponse<IEnumerable<DTO>>.ErrorInstance("Repsonse error");
            }
            return result;
        }

        public async Task<ResultResponse<DTO>> GetById(long id)
        {
            var uri = string.Format($"{ApiUrls["GetById"]}", id);
            var result = await GetResponse<ResultResponse<DTO>>(uri);
            if (result == null)
            {
                result = ResultResponse<DTO>.ErrorInstance("Repsonse error");
            }
            return result;
        }

        public async Task<ResultResponse<bool>> Save(DTO data)
        {
            var uri = ApiUrls["Save"];
            var result = await Post<ResultResponse<bool>, DTO>(uri, data);
            if (result == null)
            {
                result = ResultResponse<bool>.ErrorInstance("Repsonse error");
            }
            return result;
        }

        public async Task<ResultResponse<bool>> DeleteById(long id)
        {
            var uri = string.Format($"{ApiUrls["DeleteById"]}", id);
            var result = await Delete<ResultResponse<bool>>(uri);
            if (result == null)
            {
                result = ResultResponse<bool>.ErrorInstance("Repsonse error");
            }
            return result;
        }

        public async Task<ResultResponse<bool>> DeleteBatch(IList<long> ids)
        {
            //var result = false;
            //foreach (var item in ids)
            //{
            //    var uri = string.Format($"/user/{item}");
            //    var response = await Delete<ResultResponse<bool>>(uri);
            //    if (response.Data is bool isSuccess && isSuccess)
            //    {
            //        result = true;
            //    }
            //}

            //return new ResultResponse<bool>
            //{
            //    Code = 200,
            //    Message = "OK",
            //    Data = result
            //};

            var uri = ApiUrls["DeleteBatch"];
            var result = await DeleteWithBody<ResultResponse<bool>, IList<long>>(uri, ids);
            if (result == null)
            {
                result = ResultResponse<bool>.ErrorInstance("Repsonse error");
            }
            return result;

        }

        protected List<Tuple<string, object>> GetQueryByInput(DTO? request)
        {
            var parameters = new List<Tuple<string, object>>();
            if (request != null)
            {
                var properties = request.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(request);
                    if (value != null && !value.Equals(DefaultForType(property.PropertyType)))
                    {
                        parameters.Add(new(property.Name, value));
                    }
                }
            }

            return parameters;
        }

        public object? DefaultForType(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}
