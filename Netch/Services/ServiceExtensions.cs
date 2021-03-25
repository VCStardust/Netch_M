using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Netch.Forms;
using Netch.Models;

namespace Netch.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddViews(this IServiceCollection service)
        {
            service.AddSingleton<MainForm>();
            return service;
        }

        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            services.TryAddSingleton<Setting>();

            return services;
        }

        public static IServiceCollection AddDynamicData(this IServiceCollection services)
        {
            services.TryAddSingleton<SourceList<Mode>>();

            return services;
        }
    }
}
