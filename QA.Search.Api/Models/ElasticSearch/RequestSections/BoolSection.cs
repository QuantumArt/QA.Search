using Newtonsoft.Json;
using System.Collections.Generic;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class BoolSection
    {
        [JsonProperty("must")]
        public IEnumerable<MustSection> Must { get; set; }
    }
}
