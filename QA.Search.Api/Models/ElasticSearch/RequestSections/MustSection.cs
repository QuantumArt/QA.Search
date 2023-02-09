using Newtonsoft.Json;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class MustSection
    {
        [JsonProperty("multi_match")]
        public MultiMatchSection MultiMatches { get; set; }
    }
}
