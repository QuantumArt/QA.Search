using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class FacetsMapper
    {
        private readonly IndexMapper _indexMapper;

        public FacetsMapper(IndexMapper indexMapper)
        {
            _indexMapper = indexMapper;
        }

        /// <summary>
        /// Преобразует результаты аггрегаций Elastic в формат фасетов
        /// </summary>
        public Dictionary<string, FacetItem> MapFacets(JObject aggregations)
        {
            var facetItems = new Dictionary<string, FacetItem>(aggregations.Count);

            foreach (var (path, token) in aggregations)
            {
                JObject facetValue = (JObject)token;

                if (path == "_index:terms")
                {
                    facetItems.Add("_index", MapIndexFacet(facetValue));
                }
                else if (path.EndsWith(":min"))
                {
                    string realPath = CutSuffix(path, ":min");
                    facetItems.Add(realPath, MapIntervalFacet(realPath, aggregations));
                }
                else if (path.EndsWith(":max"))
                {
                    continue;
                }
                else if (path.EndsWith(":terms"))
                {
                    facetItems.Add(CutSuffix(path, ":terms"), MapSamplesFacet(facetValue));
                }
                else if (path.EndsWith(":percentiles"))
                {
                    facetItems.Add(CutSuffix(path, ":percentiles"), MapPercentilesFacet(facetValue));
                }
                else if (path.EndsWith(":range"))
                {
                    facetItems.Add(CutSuffix(path, ":range"), MapRangesFacet(facetValue));
                }
            }

            return facetItems;
        }

        private static string CutSuffix(string path, string suffix)
        {
            return path.Substring(0, path.Length - suffix.Length);
        }

        private FacetItem MapIndexFacet(JObject facetValue)
        {
            SampleFacet[] samples = facetValue["buckets"]
                .Cast<JObject>()
                .Select(item => new SampleFacet
                {
                    Value = (JValue)_indexMapper.ShortIndexName((string)item["key"]),
                    Count = (int)item["doc_count"],
                })
                .ToArray();

            return new FacetItem { Samples = samples };
        }

        private FacetItem MapIntervalFacet(string path, JObject aggregations)
        {
            JObject minFacetValue = (JObject)aggregations[path + ":min"];
            JObject maxFacetValue = (JObject)aggregations[path + ":max"];

            var interval = new IntervalFacet
            {
                From = (JValue)(minFacetValue["value_as_string"] ?? minFacetValue["value"]),
                To = (JValue)(maxFacetValue["value_as_string"] ?? maxFacetValue["value"]),
            };

            return new FacetItem { Interval = interval };
        }

        private FacetItem MapSamplesFacet(JObject facetValue)
        {
            SampleFacet[] samples = facetValue["buckets"]
                .Cast<JObject>()
                .Select(item => new SampleFacet
                {
                    Value = (JValue)(item["key_as_string"] ?? item["key"]),
                    Count = (int)item["doc_count"],
                })
                .ToArray();

            return new FacetItem { Samples = samples };
        }

        private FacetItem MapPercentilesFacet(JObject facetValue)
        {
            PercentileFacet[] percentiles = facetValue["values"]
                .Cast<JObject>()
                .Select(item => new PercentileFacet
                {
                    Percent = (float)item["key"],
                    Value = (JValue)(item["value_as_string"] ?? item["value"]),
                })
                .ToArray();

            return new FacetItem { Percentiles = percentiles };
        }

        private FacetItem MapRangesFacet(JObject facetValue)
        {
            RangeFacet[] ranges = facetValue["buckets"]
                .Cast<JObject>()
                .Select(item => new RangeFacet
                {
                    Name = (JValue)item["key"],
                    From = (JValue)(item["from_as_string"] ?? item["from"]),
                    To = (JValue)(item["to_as_string"] ?? item["to"]),
                    Count = (int)item["doc_count"],
                })
                .ToArray();

            return new FacetItem { Ranges = ranges };
        }
    }
}