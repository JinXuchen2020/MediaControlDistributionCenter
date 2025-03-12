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
            {"GetAll", "/programmeGroup/all"},
            {"GetPageAll", "/programmeGroup/page"},
            {"GetById", "/programmeGroup/{0}"},
            {"Save", "/programmeGroup/save"},
            {"DeleteById", "/programmeGroup/{0}"},
            {"DeleteBatch", "/programmeGroup/batch"},
        };

        public ProgramGroupService(IOptions<ConnectionMode> options) : base(options)
        {
        }
    }
}
