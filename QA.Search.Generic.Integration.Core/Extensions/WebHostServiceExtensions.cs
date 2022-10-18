using Elasticsearch.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Processors;
using QA.Search.Generic.Integration.Core.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace QA.Search.Generic.Integration.Core.Extensions
{
    public static class WebHostServiceExtensions
    {
        public static void RunAdaptive(this IWebHost host)
        {
            var options = host.Services.GetService<IOptions<CommonSettings>>().Value;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Debugger.IsAttached && options.IsService)
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }

        /// <summary>
        /// Register <see cref="ElasticLowLevelClient"/> with semicolon-separated list of ElasticSearch Cluster URLs
        /// </summary>
        /// <example>
        /// http://localhost:9200;http://localhost:9201
        /// </example>
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(CommonSettings)).Get<CommonSettings>();
            var nodes = settings.ElasticSearchUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(node => new Uri(node));
            var pool = new StaticConnectionPool(nodes);
            var config = new ConnectionConfiguration(pool).RequestTimeout(settings.RequestTimeout);
            var client = new ElasticLowLevelClient(config);
            services.AddSingleton<IElasticLowLevelClient>(client);
        }

        /// <summary>
        /// Register ScheduledService with IndexingContext and Settings as IHostedService
        /// </summary>
        public static void AddScheduledService<TService, TSettings, TContext, TMarker>(this IServiceCollection services)
           where TService : ScheduledServiceBase<TContext, TSettings, TMarker>
           where TContext : IndexingContext<TMarker>
           where TSettings : Settings<TMarker>, new()
           where TMarker : IServiceMarker
        {
            services.AddSingleton<TService>();
            services.AddSingleton<IHostedService>(s => s.GetRequiredService<TService>());
            services.AddSingleton<IServiceController<TMarker>>(s => s.GetRequiredService<TService>());
            services.AddSingleton<TContext>();
            services.AddTransient<DocumentMiddleware<TMarker>>();
        }

        /// <summary>
        /// Register DocumentProcessor that intercepts documents from specified ScheduledService marker
        /// </summary>
        public static void AddDocumentProcessor<TProcessor, TMarker>(this IServiceCollection services)
            where TProcessor : DocumentProcessor<TMarker>
            where TMarker : IServiceMarker
        {
            services.AddTransient<DocumentProcessor<TMarker>, TProcessor>();
        }
    }
}