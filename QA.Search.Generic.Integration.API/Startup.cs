using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.Integration.Core.Extensions;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Processors;
using QA.Search.Generic.Integration.QP.Extensions;
using QA.Search.Generic.Integration.QP.Markers;
using QA.Search.Generic.Integration.QP.Models;
using QA.Search.Generic.Integration.QP.Permissions.Extensions;
using QA.Search.Generic.Integration.QP.Services;
using System.Text.Json.Serialization;

namespace QA.Search.Generic.Integration.DPC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CommonSettings>(Configuration.GetSection(nameof(CommonSettings)));
            services.Configure<Settings<QpMarker>>(Configuration.GetSection("Settings.QP"));
            services.Configure<ViewOptions>(Configuration.GetSection(nameof(ViewOptions)));
            services.Configure<ContextConfiguration>(Configuration.GetSection(nameof(ContextConfiguration)));
            services.Configure<GenericIndexSettings>(Configuration.GetSection(nameof(GenericIndexSettings)));

            services.AddMemoryCache();
            services.AddHttpClient();

            services.AddScheduledService<ScheduledServiceQP, Settings<QpMarker>, IndexingQpContext, QpMarker>();
            services.AddScheduledService<ScheduledServiceQPUpdate, Settings<QpUpdateMarker>, IndexingQpUpdateContext, QpUpdateMarker>();

            services.AddDocumentProcessor<HtmlStripProcessor<QpMarker>, QpMarker>();
            services.AddDocumentProcessor<HtmlStripProcessor<QpUpdateMarker>, QpUpdateMarker>();

            services.AddElasticSearch(Configuration);
            services.AddMvc()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Search.Integration.DPC",
                    Description = "Index DPC products"
                });
            });

            services.AddSingleton<ILogger>(provider => provider.GetRequiredService<ILogger<Program>>()); //общий логгер

            services.AddDbContext<ServiceDataContext>(ServiceLifetime.Transient);

            CommonSettings commonSettings = new();
            Configuration.GetSection(nameof(CommonSettings)).Bind(commonSettings);

            if (commonSettings.IndexPermissions)
                services.AddPermissionsService(Configuration);

            if (commonSettings.IndexerLibraries is not null && commonSettings.IndexerLibraries.Length > 0)
                services.AddIndexersByAssemblyNames(Configuration, commonSettings.IndexerLibraries);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime lifetime,
            IServer server,
            ILogger<Startup> logger)
        {
            logger.LogInformation("@{Environment}", env.EnvironmentName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "Search.Integration.DPC");
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var addresses = server.Features?.Get<IServerAddressesFeature>()?.Addresses;

            logger.LogInformation("Application started: {Environment} {Addresses}", env.EnvironmentName, addresses);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Application stopped");
            });
        }
    }
}
