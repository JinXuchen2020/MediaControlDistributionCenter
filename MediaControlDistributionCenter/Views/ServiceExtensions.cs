using MediaControlDistributionCenter.Services.ApiImps;
using MediaControlDistributionCenter.Services.LocalImps;
using MediaControlDistributionCenter.Views.DeviceManagement;
using MediaControlDistributionCenter.Views.Diagrams;
using MediaControlDistributionCenter.Views.MediaManagement;
using MediaControlDistributionCenter.Views.UserManagement;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace MediaControlDistributionCenter.Views
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddPageViewServices(this IServiceCollection services)
        {
            services.AddScoped<MainWindow>();
            services.AddScoped<Login>();
            services.AddTransient<DeviceManage>();

            services.AddTransient<MediaEdit>();
            services.AddTransient<MediaPreview>();

            services.AddTransient<DeviceControlContent>();
            services.AddTransient<MediaManage>();

            services.AddTransient<UserSettingsContent>();

            services.AddTransient<MediaPublishDialog>();

            services.AddTransient<UserControllers>();

            services.AddTransient<UserManage>();

            services.AddTransient<Dashboard>();

            services.AddTransient<ResultConfirmDialog>();

            services.AddTransient<MediaContent>();
            return services;
        }
    }
}
