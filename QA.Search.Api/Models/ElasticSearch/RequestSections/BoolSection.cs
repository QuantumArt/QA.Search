using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class BoolSection
    {
        [JsonProperty("must")]
        public IEnumerable<MustSection> Must { get; set; }
    }
}
