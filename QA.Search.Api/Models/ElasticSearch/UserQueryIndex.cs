using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models.ElasticSearch
{
    public class UserQueryIndex
    {
        public string Query { get; set; }
        public string Region { get; set; }
    }
}
