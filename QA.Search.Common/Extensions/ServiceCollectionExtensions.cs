using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QA.Search.Common.Interfaces;
using QA.Search.Common.Models;
using System;
using System.Linq;

namespace QA.Search.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register <see cref="ElasticLowLevelClient"/> with semicolon-separated list of ElasticSearch Cluster URLs
        /// </summary>
        /// <example>
        /// http://localhost:9200;http://localhost:9201
        /// </example>
        public static void AddElasticSearch(this IServiceCollection services)
        {
            services.AddSingleton<IElasticLowLevelClient>(sp =>
            {
                ElasticSettings elasticSettings = sp.GetRequiredService<IOptions<ElasticSettings>>().Value;

                var nodes = ParseUrls(elasticSettings.Address);

                StaticConnectionPool pool = new(nodes);
                ConnectionConfiguration config = new ConnectionConfiguration(pool)
                    .RequestTimeout(elasticSettings.RequestTimeout);

                if (elasticSettings.BasicAuth != null && !string.IsNullOrWhiteSpace(elasticSettings.BasicAuth.User))
                    config = config.BasicAuthentication(elasticSettings.BasicAuth.User, elasticSettings.BasicAuth.Password);

                ElasticLowLevelClient client = new(config);

                return client;
            });
        }

        private static Uri[] ParseUrls(string url)
        {
            try
            {
                return url
                    .Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .Select(node => new Uri(node))
                    .ToArray();
            }
            catch
            {
                return new[] { new Uri("http://localhost:9200") };
            }
        }

        public static void AddElasticConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ElasticSettings>(configuration.GetSection(nameof(ElasticSettings)));
            services.AddSingleton<IElasticSettingsProvider, ElasticSettingsProvider>();
        }
    }
}