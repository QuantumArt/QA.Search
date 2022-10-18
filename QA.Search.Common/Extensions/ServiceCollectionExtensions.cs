using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
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
        public static void AddElasticSearch(this IServiceCollection services, string url)
        {
            var urls = ParseUrls(url);
            var pool = new StaticConnectionPool(urls);
            var config = new ConnectionConfiguration(pool);
            var client = new ElasticLowLevelClient(config);
            services.AddSingleton<IElasticLowLevelClient>(client);
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
    }
}