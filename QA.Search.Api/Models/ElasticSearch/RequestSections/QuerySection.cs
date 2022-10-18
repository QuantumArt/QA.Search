using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class QuerySection
    {
        [JsonProperty("multi_match")]
        public MultiMatchSection MultiMatch { get; set; }

        [JsonProperty("bool")]
        public BoolSection Bool { get; set; }
    }
}
