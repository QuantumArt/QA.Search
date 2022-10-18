using QA.Search.Api.Models.ElasticSearch;
using QA.Search.Api.Models.ElasticSearch.RequestSections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Builders
{
    public class QuerySectionBuilder
    {
        private List<MultiMatchSection> multiMatches;

        public void AddMultiMatches(params MultiMatchSection[] multiMatches)
        {
            this.multiMatches = this.multiMatches ?? new List<MultiMatchSection>();
            this.multiMatches.AddRange(multiMatches);
        }

        public QuerySection Build()
        {
            var result = new QuerySection();
            if (multiMatches != null)
            {
                if (multiMatches.Count() == 1)
                {
                    result.MultiMatch = multiMatches.First();
                }
                else
                {
                    result.Bool = new BoolSection
                    {
                        Must = multiMatches.Select(m => new MustSection { MultiMatches = m })
                    };
                }
            }

            return result;
        }
    }
}
