using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.LocalImps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace MediaControlDistributionCenter.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLocalServices(this IServiceCollection services)
        {
            services.AddKeyedScoped<IAuthService, AuthServiceLocal>("Local");
            services.AddKeyedScoped<IFileService, FileServiceLocal>("Local");
            AddKeyedServices(services, "Local");
            return services;
        }

        public static IServiceCollection AddRemoteServices(this IServiceCollection services)
        {
            services.AddKeyedScoped<IAuthService, AuthService>("Remote");
            services.AddKeyedScoped<IFileService, FileService>("Remote");
            services.AddHttpClient();
            AddKeyedServices(services, "Remote");
            return services;
        }

        private static void AddKeyedServices(IServiceCollection services, string key)
        {
            var types = typeof(IService<,>).Assembly.DefinedTypes;
            var interfaceTypes = types.Where(c => c.IsInterface && !c.IsGenericType);

            foreach (var interfaceType in interfaceTypes)
            {
                var implementTypes = types.Where(c => c.ImplementedInterfaces.Count() > 0 && c.ImplementedInterfaces.Contains(interfaceType));
                foreach (var implementType in implementTypes)
                {
                    bool isLocal = implementType.Name.EndsWith("Local");
                    if ((key == "Local" && isLocal) || (key == "Remote" && !isLocal))
                    {
                        services.AddKeyedScoped(interfaceType, key, implementType);
                    }
                }
            }
        }
    }
}
