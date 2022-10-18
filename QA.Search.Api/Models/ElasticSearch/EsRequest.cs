using Newtonsoft.Json;
using QA.Search.Api.Models.ElasticSearch.RequestSections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models.ElasticSearch
{
    public class EsRequest
    {
        [JsonProperty("size")]
        public int? Size { get; set; }

        [JsonProperty("query")]
        public QuerySection Query { get; set; }

        [JsonProperty("aggs")]
        public AggsSection Aggs { get; set; }
    }
}
