using MediaControlDistributionCenter.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public class Utility
    {
        public static T GetService<T>() where T : class
        {
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            var key = connectionMode.Mode == "Local" || string.IsNullOrEmpty(connectionMode.ServiceUri) ? "Local" : "Remote";
            return App.ServicesProvider.GetRequiredKeyedService<T>(key);
        }

        public static string FindResource(string key)
        {
            return (string)LanguageTool.Instance.FindResource(key);
        }
        public static string GetSizeText(double? fileSize)
        {
            return fileSize != null && fileSize > 0 ? $"{Math.Round(fileSize.Value / 1024 / 1024, 2)}MB" : string.Empty;
        }
    }
}
