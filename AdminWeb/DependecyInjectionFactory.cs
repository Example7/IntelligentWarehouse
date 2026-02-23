using Interfaces.CMS;
using Interfaces.Magazyn;
using Services.CMS;
using Services.Magazyn;

namespace AdminWeb
{
    public static class DependecyInjectionFactory
    {
        public static void Resolve(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAktualnoscService, AktualnoscService>();
            services.AddScoped<IStronaService, StronaService>();
            services.AddScoped<IProduktService, ProduktService>();
            services.AddScoped<IKategoriaService, KategoriaService>();
            services.AddScoped<ILokacjaService, LokacjaService>();
        }
    }
}
