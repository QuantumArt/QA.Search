using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System;

namespace QA.Search.Api.Services
{
    public class SearchTranspiler
    {
        private readonly FilterTranspiler _filterTranspiler;
        private readonly QueryTranspiler _queryTranspiler;
        private readonly SnippetsTranspiler _snippetsTranspiler;
        private readonly FacetsTranspiler _facetsTranspiler;

        public SearchTranspiler(
            FilterTranspiler filterTranspiler,
            QueryTranspiler queryTranspiler,
            SnippetsTranspiler snippetsTranspiler,
            FacetsTranspiler facetsTranspiler)
        {
            _filterTranspiler = filterTranspiler;
            _queryTranspiler = queryTranspiler;
            _snippetsTranspiler = snippetsTranspiler;
            _facetsTranspiler = facetsTranspiler;
        }

        /// <summary>
        /// Преобразует запрос для полнотекстового поиска документов
        /// с синонимами и морфологией, а также фасетного поиска в Elastic Query DSL
        /// </summary>
        public JObject Transpile(SearchRequest request)
        {
            var query = new JObject()
            {
                ["track_total_hits"] = true,
            };

            if (request.Offset != null)
            {
                query["from"] = request.Offset;
            }
            if (request.Limit != null)
            {
                query["size"] = request.Limit;
            }
            if (request.Select != null)
            {
                query["_source"] = new JArray(request.Select);
            }
            if (request.OrderBy != null)
            {
                query["sort"] = request.OrderBy;
            }
            if (request.Snippets != null)
            {
                query["highlight"] = _snippetsTranspiler.BuildSearchSnippets(request.Snippets);
            }
            if (request.Where != null)
            {
                var boolQuery = new JObject
                {
                    ["filter"] = _filterTranspiler.BuildWhere(request.Where),
                };

                if (!String.IsNullOrWhiteSpace(request.Query))
                {
                    boolQuery["must"] = _queryTranspiler.BuildSearchQuery(
                        request.Query, request.RequiredWordsCount, request.Weights);
                }

                query["query"] = Bool(boolQuery);
            }
            else if (!String.IsNullOrWhiteSpace(request.Query))
            {
                query["query"] = _queryTranspiler.BuildSearchQuery(
                    request.Query, request.RequiredWordsCount, request.Weights);
            }
            if (request.Facets != null)
            {
                query["aggregations"] = _facetsTranspiler.BuildFacets(request.Facets);
            }
            return query;
        }

        private static JObject Bool(JToken boolQuery)
        {
            return new JObject
            {
                ["bool"] = boolQuery
            };
        }
    }
}