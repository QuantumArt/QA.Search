using System;

namespace QA.Search.Api
{
    public class Settings
    {
        /// <summary>
        /// Semicolon-separated list of ElasticSearch Cluster URLs
        /// </summary>
        /// <example>
        /// http://localhost:9200;http://localhost:9201
        /// </example>
        public string ElasticSearchUrl { get; set; }

        /// <summary>
        /// Prefix for all ElasticSearch indexes that used by Search App
        /// </summary>
        public string IndexPrefix { get; set; } = "";

        /// <summary>
        /// Wildcard mask for all ElasticSearch indexes that used by Search App
        /// </summary>
        public string IndexMask => IndexPrefix + "*";

        /// <summary>
        /// Prefix for all ElasticSearch aliases that used by Search App
        /// </summary>
        public string AliasPrefix { get; set; } = "";

        /// <summary>
        /// Wildcard mask for all ElasticSearch aliases that used by Search App
        /// </summary>
        public string AliasMask => AliasPrefix + "*";

        /// <summary>
        /// ElasticSearch Document fields which values depends on request $where clause
        /// </summary>
        /// <example>
        /// "SearchUrl" that depends on "Regions" field
        /// </example>
        public string[] ContextualFields { get; set; } = Array.Empty<string>();

        /// <summary>
        /// ElasticSearch index for user queries
        /// </summary>
        public string UserQueryIndex { get; set; }

        /// <summary>
        /// User query default suggestions length
        /// </summary>
        public int SuggestionsDefaultLength { get; set; }

        /// <summary>
        /// Name of index which store role permissions
        /// </summary>
        public string PermissionsIndexName { get; set; }

        /// <summary>
        /// Use permissions or process query to all indexes without limitations
        /// </summary>
        public bool UsePermissions { get; set; }

        /// <summary>
        /// Роль по умолчанию для публичных разделов.
        /// </summary>
        public string DefaultReaderRole { get; set; }
    }
}
