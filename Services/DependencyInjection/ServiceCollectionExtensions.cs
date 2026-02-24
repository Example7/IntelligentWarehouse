using Interfaces.CMS;
using Interfaces.Dashboard;
using Interfaces.Magazyn;
using Microsoft.Extensions.DependencyInjection;
using Services.CMS;
using Services.Dashboard;
using Services.Magazyn;

namespace Services.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCmsAppServices(this IServiceCollection services)
        {
            services.AddScoped<IAktualnoscService, AktualnoscService>();
            services.AddScoped<IStronaService, StronaService>();

            return services;
        }

        public static IServiceCollection AddWarehouseAppServices(this IServiceCollection services)
        {
            services.AddScoped<IProduktService, ProduktService>();
            services.AddScoped<IKategoriaService, KategoriaService>();
            services.AddScoped<IJednostkaMiaryService, JednostkaMiaryService>();
            services.AddScoped<ILokacjaService, LokacjaService>();
            services.AddScoped<IMagazynService, MagazynService>();

            return services;
        }

        public static IServiceCollection AddDashboardAppServices(this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
