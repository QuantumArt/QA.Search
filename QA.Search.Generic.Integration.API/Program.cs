using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QA.Search.Generic.Integration.Core.Extensions;
using System.Diagnostics;
using System.IO;

namespace QA.Search.Generic.Integration.DPC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            if (File.Exists(Path.Combine(pathToContentRoot, "appsettings.json")))
            {
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            CreateWebHost(args).RunAdaptive();
        }

        public static IWebHost CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .SuppressStatusMessages(true)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .Build();
    }
}
