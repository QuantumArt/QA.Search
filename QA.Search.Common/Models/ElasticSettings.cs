using System;

namespace QA.Search.Common.Models
{
    public class ElasticSettings
    {
        /// <summary>
        /// Semicolon-separated list of ElasticSearch Cluster URLs
        /// </summary>
        /// <example>
        /// http://localhost:9200;http://localhost:9201
        /// </example>
        public string Address { get; set; }
        /// <summary>
        /// Name of project which indexades
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// Timeout for ElasticSearch requests
        /// </summary>
        public TimeSpan RequestTimeout { get; set; }
    }
}
