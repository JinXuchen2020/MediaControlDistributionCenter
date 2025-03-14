using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace MediaControlDistributionCenter.ViewModels
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddPageViewModelServices(this IServiceCollection services)
        {
            var types = typeof(PageViewModel).Assembly.DefinedTypes;
            var pageViewTypes = types.Where(c => c.BaseType == typeof(PageViewModel));

            foreach(var type in pageViewTypes)
            {
                services.AddScoped(type);
            }

            return services;
        }
    }
}
