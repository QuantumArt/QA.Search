using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.IndexesInfoParsing
{
    public class IndexesInfoParserSettings
    {
        public string IndexParseRegexTemplate { get; set; } 
        public string DateTimeParsingFormat { get; set; }
    }
}
