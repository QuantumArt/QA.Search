using Newtonsoft.Json;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class TermsSection
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("size")]
        public int? Size { get; set; }
    }

}
