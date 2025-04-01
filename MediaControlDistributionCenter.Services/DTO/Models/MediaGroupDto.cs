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
    public class MediaGroupDto : BaseDto, IMappingProfile<MediaGroup>
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        public void CreateMappings(Profile configuration)
        {
            configuration.CreateMap<MediaGroup, MediaGroupDto>();
        }

        public MediaGroup ToModel()
        {
            return new MediaGroup
            {
                Id = (int)Id,
                Name = Name
            };
        }
    }
}
