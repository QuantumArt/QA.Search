using Newtonsoft.Json;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class AggsSectionValue
    {
        [JsonProperty("terms")]
        public TermsSection Terms { get; set; }
    }
}
