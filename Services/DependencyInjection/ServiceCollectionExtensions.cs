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
            services.AddScoped<IPlikMediaService, PlikMediaService>();
            services.AddScoped<ISzablonWydrukuService, SzablonWydrukuService>();
            services.AddScoped<IZalacznikDokumentuService, ZalacznikDokumentuService>();

            return services;
        }

        public static IServiceCollection AddWarehouseAppServices(this IServiceCollection services)
        {
            services.AddScoped<IProduktService, ProduktService>();
            services.AddScoped<IKodProduktuService, KodProduktuService>();
            services.AddScoped<IProduktJednostkaService, ProduktJednostkaService>();
            services.AddScoped<IPartiaService, PartiaService>();
            services.AddScoped<IDokumentPZService, DokumentPZService>();
            services.AddScoped<IPozycjaPZService, PozycjaPZService>();
            services.AddScoped<IDokumentWZService, DokumentWZService>();
            services.AddScoped<IPozycjaWZService, PozycjaWZService>();
            services.AddScoped<IDokumentMMService, DokumentMMService>();
            services.AddScoped<IPozycjaMMService, PozycjaMMService>();
            services.AddScoped<IInwentaryzacjaService, InwentaryzacjaService>();
            services.AddScoped<IPozycjaInwentaryzacjiService, PozycjaInwentaryzacjiService>();
            services.AddScoped<IRezerwacjaService, RezerwacjaService>();
            services.AddScoped<IPozycjaRezerwacjiService, PozycjaRezerwacjiService>();
            services.AddScoped<IDostawcaService, DostawcaService>();
            services.AddScoped<IKlientService, KlientService>();
            services.AddScoped<IKategoriaService, KategoriaService>();
            services.AddScoped<IJednostkaMiaryService, JednostkaMiaryService>();
            services.AddScoped<ILokacjaService, LokacjaService>();
            services.AddScoped<IMagazynService, MagazynService>();
            services.AddScoped<IStanMagazynowyService, StanMagazynowyService>();
            services.AddScoped<IRaportMagazynowyService, RaportMagazynowyService>();
            services.AddScoped<IRuchMagazynowyService, RuchMagazynowyService>();
            services.AddScoped<IRegulaAlertuService, RegulaAlertuService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<ILogAudytuService, LogAudytuService>();
            services.AddScoped<IUstawienieAplikacjiService, UstawienieAplikacjiService>();
            services.AddScoped<IUzytkownikService, UzytkownikService>();
            services.AddScoped<IRolaService, RolaService>();
            services.AddScoped<IUzytkownikRolaService, UzytkownikRolaService>();

            return services;
        }

        public static IServiceCollection AddDashboardAppServices(this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
