using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QA.Search.Common
{
    public static class Settings
    {
        public static readonly JsonSerializerSettings JsonCamelCaseSerializer = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static class ElasticSearch
        {
            public const string DocType = "_doc";
            public const string AggregationsFieldName = "aggregations";
            public const string SuggestionsFieldName = "suggestions";
            public const string BucketsFieldName = "buckets";

            public static class ErrorType
            {
                public const string IndexNotFound = "index_not_found_exception";
            }
        }
    }
}
