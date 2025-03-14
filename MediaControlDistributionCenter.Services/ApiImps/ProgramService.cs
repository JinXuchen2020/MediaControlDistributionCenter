using MediaControlDistributionCenter.Data.Entity;
using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class ProgramService : BaseService<Program, ProgramDto>, IProgramService
    {
        public override Dictionary<string, string> ApiUrls => new Dictionary<string, string>
        {
            {"GetAll", "/programme/all"},
            {"GetPageAll", "/programme/page"},
            {"GetById", "/programme/{0}"},
            {"Save", "/programme/save"},
            {"DeleteById", "/programme/{0}"},
            {"DeleteBatch", "/programme/batch"},
        };

        public ProgramService(ConnectionMode options) : base(options)
        {
        }
    }
}
