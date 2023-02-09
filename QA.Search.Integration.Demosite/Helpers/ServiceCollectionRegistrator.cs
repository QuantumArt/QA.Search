using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Interfaces;
using QA.Search.Generic.Integration.QP.Extensions;
using QA.Search.Generic.Integration.QP.Interfaces;
using QA.Search.Integration.Demosite.Models;
using QA.Search.Integration.Demosite.Services;

namespace QA.Search.Integration.Demosite.Helpers
{
    public class ServiceCollectionRegistrator : IServiceRegistrator
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("demosite.json", false, true).Build();

            services.Configure<DemositeSettings>(config.GetSection(nameof(DemositeSettings)));
            services.AddDbContext<DemositeDataContext>(ServiceLifetime.Transient);
            services.AddSingleton<IUrlService<DemositeDataContext>, UrlService<DemositeDataContext>>();
            services.AddElasticViews<GenericDataContext>();
        }
    }
}
