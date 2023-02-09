using System;

namespace QA.Search.Api
{
    public class Settings
    {
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
