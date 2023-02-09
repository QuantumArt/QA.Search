using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QA.Search.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();

            await AutoMigrateContext(host);

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .SuppressStatusMessages(true)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();

        private static async Task AutoMigrateContext(IWebHost host)
        {
            using IServiceScope scope = host.Services.GetService<IServiceScopeFactory>().CreateScope();

            using AdminSearchDbContext searchDbContext = scope.ServiceProvider.GetRequiredService<AdminSearchDbContext>();

            if ((await searchDbContext.Database.GetPendingMigrationsAsync()).Any())
                await searchDbContext.Database.MigrateAsync();

            using CrawlerSearchDbContext crawlerSearchDbContext = scope.ServiceProvider.GetRequiredService<CrawlerSearchDbContext>();

            if ((await crawlerSearchDbContext.Database.GetPendingMigrationsAsync()).Any())
                await crawlerSearchDbContext.Database.MigrateAsync();
        }
    }
}