using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class ProgramGroupDto : BaseDto, IMappingProfile<ProgramGroup>
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("agentAccount", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAccount { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<ProgramGroup, ProgramGroupDto>();
        }

        public ProgramGroup ToModel()
        {
            return new ProgramGroup
            {
                Id = (int)Id,
                Name = Name,
                UserAccount = UserAccount,
            };
        }
    }
}
