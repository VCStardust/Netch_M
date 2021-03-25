using Microsoft.Extensions.DependencyInjection;
using Splat;

namespace Netch.Services
{
    public static class DependencyInjection
    {
        public static T GetService<T>()
        {
            return Locator.Current.GetService<T>();
        }

        public static void Register()
        {
            var services = new ServiceCollection();

            Locator.CurrentMutable.InitializeSplat();

            ConfigureServices(services);
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddViews();
            services.AddConfig();
            services.AddDynamicData();
            return services;
        }
    }
}
