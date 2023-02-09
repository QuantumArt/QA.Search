using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QA.Search.Api.BLL;
using QA.Search.Api.Services;
using QA.Search.Common.Extensions;
using System.Text.Json.Serialization;

namespace QA.Search.Api
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
            services.Configure<Settings>(Configuration.GetSection(nameof(Settings)));

            var settings = Configuration.GetSection(nameof(Settings)).Get<Settings>();

            services.AddResponseCompression();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins("https://*.dev.qsupport.ru")
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .Build());
            });

            services
                .AddMvc()
                .AddNewtonsoftJson()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });


            // Register the Swagger services
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Search API";
                };
            });

            services.AddElasticSearch();

            services.AddTransient<FilterTranspiler>();
            services.AddTransient<QueryTranspiler>();
            services.AddTransient<SnippetsTranspiler>();
            services.AddTransient<FacetsTranspiler>();
            services.AddTransient<SearchTranspiler>();
            services.AddTransient<SuggestTranspiler>();
            services.AddTransient<CorrectionTranspiler>();
            services.AddTransient<CompletionTranspiler>();
            services.AddTransient<IndexMapper>();
            services.AddTransient<FacetsMapper>();
            services.AddTransient<SearchMapper>();
            services.AddTransient<SnippetsMapper>();
            services.AddTransient<SuggestMapper>();
            services.AddTransient<CorrectionMapper>();
            services.AddTransient<CompletionMapper>();
            services.AddTransient<ContextualFieldsMapper>();
            services.AddTransient<CompletionService>();

            services.AddElasticConfiguration(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime lifetime,
            IServer server,
            ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseResponseCompression();
            }

            app.UseCors();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3(options => options.Path = "/swagger");
            app.UseReDoc(options => options.Path = "/redoc");

            app.UseStaticFiles();

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