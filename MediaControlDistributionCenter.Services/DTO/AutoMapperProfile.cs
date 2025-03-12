using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaControlDistributionCenter.Services.DTO
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            var maps = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
              .Where(x => typeof(IMappingProfile<>).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
              .Select(x => (IMappingProfile<BaseModel>)Activator.CreateInstance(x)!).ToList();

            foreach (var map in maps)
            {
                map.CreateMappings(this);
            }

        }
    }
}
