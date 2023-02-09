using Newtonsoft.Json;
using System.Collections.Generic;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class MultiMatchSection
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("fields")]
        public List<string> Fields { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("minimum_should_match")]
        public string MinimumShouldMatch { get; set; }
    }

}
