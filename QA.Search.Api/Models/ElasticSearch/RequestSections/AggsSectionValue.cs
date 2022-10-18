using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models.ElasticSearch.RequestSections
{
    public class AggsSectionValue
    {
        [JsonProperty("terms")]
        public TermsSection Terms { get; set; }
    }
}
