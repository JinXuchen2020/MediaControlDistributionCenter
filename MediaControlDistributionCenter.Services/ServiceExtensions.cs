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
            services.AddScoped<IAuthService, AuthServiceLocal>();
            services.AddScoped<IFileService, FileServiceLocal>();

            var types = typeof(IService<,>).Assembly.DefinedTypes;
            var interfaceTypes = types.Where(c => c.IsInterface && !c.IsGenericType);

            foreach(var interfaceType in interfaceTypes)
            {
                var implementTypes = types.Where(c => c.ImplementedInterfaces.Count() > 0 && c.ImplementedInterfaces.Contains(interfaceType));
                foreach (var implementType in implementTypes)
                {
                    if (implementType != null && implementType.Name.EndsWith("Local"))
                    {
                        services.AddScoped(interfaceType, implementType);
                    }
                }
            }

            return services;
        }

        public static IServiceCollection AddRemoteServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileService, FileServiceLocal>();

            services.Configure<ConnectionMode>((c) => configuration.Bind("ConnectionMode", c));

            var types = typeof(IService<,>).Assembly.DefinedTypes;
            var interfaceTypes = types.Where(c => c.IsInterface && !c.IsGenericType);

            foreach (var interfaceType in interfaceTypes)
            {
                var implementTypes = types.Where(c => c.ImplementedInterfaces.Count() > 0 && c.ImplementedInterfaces.Contains(interfaceType));
                foreach (var implementType in implementTypes)
                {
                    if (implementType != null && !implementType.Name.EndsWith("Local"))
                    {
                        services.AddScoped(interfaceType, implementType);
                    }
                }
            }

            return services;
        }
    }
}
