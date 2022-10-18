using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;

namespace QA.Search.Api.Services
{
    public class FacetsTranspiler
    {
        /// <summary>
        /// Преобразует определения фасетов в синтаксис Elastic "aggregations":
        /// $interval — в "min" и "max",
        /// $samples — в "terms",
        /// $ranges — в "ranges",
        /// $percentiles — в "percentiles".
        /// </summary>
        public JObject BuildFacets(FacetsExpression facets)
        {
            var aggregations = new JObject();

            foreach (var (field, facet) in facets)
            {
                if (facet.Interval)
                {
                    aggregations[$"{field}:min"] = new JObject
                    {
                        ["min"] = new JObject { ["field"] = field }
                    };
                    aggregations[$"{field}:max"] = new JObject
                    {
                        ["max"] = new JObject { ["field"] = field }
                    };
                }
                else if (facet.Samples != null)
                {
                    aggregations[$"{field}:terms"] = new JObject
                    {
                        ["terms"] = new JObject
                        {
                            ["field"] = field,
                            ["size"] = facet.Samples,
                        }
                    };
                }
                else if (facet.Percentiles != null)
                {
                    aggregations[$"{field}:percentiles"] = new JObject
                    {
                        ["percentiles"] = new JObject
                        {
                            ["field"] = field,
                            ["keyed"] = false,
                            ["percents"] = new JArray(facet.Percentiles),
                        }
                    };
                }
                else if (facet.Ranges != null)
                {
                    aggregations[$"{field}:range"] = BuildRangesFacet(field, facet);
                }
            }

            return aggregations;
        }

        private static JObject BuildRangesFacet(string field, FacetExpression facet)
        {
            var ranges = new JArray();

            foreach (RangeFacetExpression item in facet.Ranges)
            {
                var range = new JObject { ["key"] = item.Name };

                if (item.From != null)
                {
                    range["from"] = item.From;
                }
                if (item.To != null)
                {
                    range["to"] = item.To;
                }

                ranges.Add(range);
            }

            return new JObject
            {
                ["range"] = new JObject
                {
                    ["field"] = field,
                    ["ranges"] = ranges,
                }
            };
        }
    }
}