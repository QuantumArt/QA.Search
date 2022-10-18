using System;

namespace QA.Search.Generic.Integration.Core.Models
{
    public class CommonSettings
    {
        /// <summary>
        /// Run as Windows Service or as Console App
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Semicolon-separated list of ElasticSearch Cluster URLs
        /// </summary>
        /// <example>
        /// http://localhost:9200;http://localhost:9201
        /// </example>
        public string ElasticSearchUrl { get; set; }

        /// <summary>
        /// Timeout for ElasticSearch requests
        /// </summary>
        public TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// Indicates index permissions or not
        /// </summary>
        public bool IndexPermissions { get; set; }

        /// <summary>
        /// Array of indexer library names (without extension)
        /// </summary>
        public string[] IndexerLibraries { get; set; }
    }
}