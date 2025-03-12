using AutoMapper;
using MediaControlDistributionCenter.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaControlDistributionCenter.Services.DTO
{
    public interface IMappingProfile<T>
    {
        void CreateMappings(Profile configuration);

        T ToModel();
    }
}
