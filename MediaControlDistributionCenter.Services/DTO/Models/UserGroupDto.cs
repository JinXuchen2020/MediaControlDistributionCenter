using AutoMapper;
using AutoMapper.Features;
using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class UserGroupDto : BaseDto, IMappingProfile<UserGroup>
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public string AgentAccount { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<UserGroup, UserGroupDto>();
        }

        public UserGroup ToModel()
        {
            return new UserGroup
            {
                Id = (int)Id,
                Name = Name,
                AgentAccount = AgentAccount,
            };
        }
    }
}
