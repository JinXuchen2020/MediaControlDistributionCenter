using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class ProgramGroupService : BaseService<ProgramGroup, ProgramGroupDto>, IProgramGroupService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/programGroup/all"},
            {"GetPageAll", "/programGroup/page"},
            {"GetById", "/programGroup/{0}"},
            {"Save", "/programGroup/save"},
            {"DeleteById", "/programGroup/{0}"},
            {"DeleteBatch", "/programGroup/batch"},
        };

        public ProgramGroupService(ConnectionMode options) : base(options)
        {
        }
    }
}
