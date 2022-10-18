using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class MustSection
    {
        [JsonProperty("multi_match")]
        public MultiMatchSection MultiMatches { get; set; }
    }
}
