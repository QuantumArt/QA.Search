using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QA.Search.Generic.Integration.Core.Helpers;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Processors;
using QA.Search.Generic.Integration.Core.Services;
using System.Diagnostics;
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
            services.TryAddSingleton<ScheduledServiceSynchronization>();
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