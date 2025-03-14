using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IService<Model, DTO>
    {
        public Task<ResultResponse<IEnumerable<DTO>>> GetAll(DTO? request);

        public Task<ResultResponse<IEnumerable<DTO>>> GetPageAll(int pageSize, int page, DTO? request);

        public Task<ResultResponse<DTO>> GetById(long id);

        public Task<ResultResponse<bool>> Save(DTO data);

        public Task<ResultResponse<bool>> DeleteById(long id);

        public Task<ResultResponse<bool>> DeleteBatch(IList<long> ids);
    }
}
