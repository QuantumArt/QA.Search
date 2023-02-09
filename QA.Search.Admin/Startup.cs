using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QA.Search.Admin.Errors;
using QA.Search.Admin.Services;
using QA.Search.Admin.Services.ElasticManagement;
using QA.Search.Admin.Services.ElasticManagement.IndexesInfoParsing;
using QA.Search.Admin.Services.ElasticManagement.Reindex;
using QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement;
using QA.Search.Admin.Services.IndexingApi;
using QA.Search.Common.Extensions;
using QA.Search.Data;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QA.Search.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        private IWebHostEnvironment HostingEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(Configuration.GetSection(nameof(Settings)));
            services.Configure<SmtpServiceSettings>(Configuration.GetSection(nameof(SmtpServiceSettings)));

            var settings = Configuration.GetSection(nameof(Settings)).Get<Settings>();

            services
                .AddMvc(options =>
                {
                    if (!HostingEnvironment.IsDevelopment())
                    {
                        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    }
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger>();

                        logger.LogWarning("Request \"{@RouteData}\" not valid {@ModelState}.", context.RouteData, context.ModelState);

                        return new BadRequestObjectResult(context.ModelState);
                    };
                })
                .AddNewtonsoftJson()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSingleton<ILogger>(provider => provider.GetRequiredService<ILogger<Program>>()); //общий логгер

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = true;
            });

            // Register the Swagger services
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Search Admin App";
                };
            });

            services.AddElasticConfiguration(Configuration);
            services.AddElasticSearch();
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AuthenticationService.Scheme;
                    options.DefaultSignInScheme = AuthenticationService.Scheme;
                })
                .AddCookie(AuthenticationService.Scheme, options =>
                {
                    options.Events.OnRedirectToLogin = (context) =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                });

            services.AddDbContext<AdminSearchDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("AdminSearchDbContextConnection"))
                .UseSnakeCaseNamingConvention());

            services.AddDbContext<CrawlerSearchDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("CrawlerSearchDbContextConnection"))
                .UseSnakeCaseNamingConvention());

            // Контекст для операций, которые выполняются вне Web запроса (Фоновый обработчик)

            services.AddScoped<AuthenticationService>();
            services.AddScoped<UsersService>();
            services.AddScoped<SmtpService>();

            services.Configure<IndexingApiServiceConfiguration>(Configuration.GetSection("IndexingApiServiceConfiguration"));
            services.AddScoped<IndexingApiServiceConfigurationSource>();
            services.AddScoped<QpIndexingApiService>();
            services.AddScoped<ElasticManagementService>();

            // То, что относится к управлению индексами Elastic
            //services.AddSingleton<IHostedService, ReindexWorker>();
            //services.AddTransient<IHostedServiceAccessor<ReindexWorker>, HostedServiceAccessor<ReindexWorker>>();


            services.AddSingleton<ReindexWorker>();

            services.AddSingleton<IHostedService, ReindexWorker>(sp => sp.GetRequiredService<ReindexWorker>());


            services.Configure<ReindexWorkerSettings>(Configuration.GetSection("ReindexWorkerSettings"));

            services.AddSingleton<ElasticConnector>();
            services.AddSingleton<ReindexTaskManager>();
            services.AddTransient<ReindexWorkerIterationProcessor>();
            services.AddSingleton<Func<ReindexWorkerIterationProcessor>>((sp) => sp.GetRequiredService<ReindexWorkerIterationProcessor>);
            // services.AddHostedService<ReindexWorker>();

            services.Configure<IndexesInfoParserSettings>(Configuration.GetSection("IndexesInfoParserSettings"));
            services.AddSingleton<IndexesInfoParser>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IAntiforgery antiForgery,
            IHostApplicationLifetime lifetime,
            IServer server,
            ILogger<Startup> logger)
        {
            app.UseExceptionHandler(ErrorHandler.Handle);

            if (!env.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3(options => options.Path = "/swagger");
            app.UseReDoc(options => options.Path = "/redoc");

            app.UseStaticFiles();
            app.UseSpaStaticFiles();


            app.Use(async (context, next) =>
            {
                string path = context.Request.Path.Value;

                if (path == null || !path.Contains("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    SetAntiForgeryCookie(context, antiForgery);
                }

                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-Xss-Protection", "1");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                await next.Invoke();

                if (context.Response.StatusCode == 200 && (
                    path.Contains("/api/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains("/api/Account/Logout", StringComparison.OrdinalIgnoreCase)))
                {
                    SetAntiForgeryCookie(context, antiForgery);
                }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            var addresses = server.Features?.Get<IServerAddressesFeature>()?.Addresses;

            logger.LogInformation("Application started: {Environment} {Addresses}", env.EnvironmentName, addresses);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Application stopped");
            });
        }

        static void SetAntiForgeryCookie(HttpContext context, IAntiforgery antiForgery)
        {
            var tokens = antiForgery.GetAndStoreTokens(context);

            context.Response.Cookies.Append(
                "XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false });
        }
    }
}