using QA.Search.Generic.Integration.Core.Services;

namespace QA.Search.Generic.Integration.Core.Models
{
    /// <summary>
    /// Settings for single integration (indexing) process
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    public class Settings<TMarker>
        where TMarker : IServiceMarker
    {
        /// <summary>
        /// CronTab string for schedule indexing. See https://crontab.guru
        /// </summary>
        public string CronSchedule { get; set; }

        /// <summary>
        /// Prefix for all ElasticSearch indexes that used by Search App
        /// </summary>
        public string IndexPrefix { get; set; }

        /// <summary>
        /// Wildcard mask for all ElasticSearch indexes that used by Search App
        /// </summary>
        public string IndexMask => IndexPrefix + "*";

        /// <summary>
        /// Prefix for all ElasticSearch aliases that used by Search App
        /// </summary>
        public string AliasPrefix { get; set; }

        /// <summary>
        /// Wildcard mask for all ElasticSearch aliases that used by Search App
        /// </summary>
        public string AliasMask => AliasPrefix + "*";

        /// <summary>
        /// Get lowercase DPC document type or QP table name, etc. in lowercase by Elastic index name
        /// and indexing context
        /// </summary>
        public string GetDocumentType(string fullIndexName, IndexingContext<TMarker> context)
        {
            int prefixLength = IndexPrefix.Length;
            int suffixLength = $".{context.GetDateSuffix()}".Length;

            return fullIndexName.Substring(prefixLength, fullIndexName.Length - prefixLength - suffixLength);
        }

        /// <summary>
        /// Get full Elastic index name by <paramref name="documentType"/> and indexing start date
        /// that stored in <paramref name="context"/>
        /// </summary>
        public string GetIndexName(string documentType, IndexingContext<TMarker> context)
        {
            return $"{IndexPrefix}{documentType}.{context.GetDateSuffix()}".ToLower();
        }

        /// <summary>
        /// Get full Elastic Alias name by <paramref name="documentType"/>
        /// </summary>
        public string GetAliasName(string documentType)
        {
            return $"{AliasPrefix}{documentType}".ToLower();
        }
    }
}