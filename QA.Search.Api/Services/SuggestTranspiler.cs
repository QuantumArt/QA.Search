using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System;

namespace QA.Search.Api.Services
{
    public class SuggestTranspiler
    {
        private readonly FilterTranspiler _filterTranspiler;
        private readonly QueryTranspiler _queryTranspiler;
        private readonly SnippetsTranspiler _snippetsTranspiler;

        public SuggestTranspiler(
            FilterTranspiler filterTranspiler,
            QueryTranspiler queryTranspiler,
            SnippetsTranspiler snippetsTranspiler)
        {
            _filterTranspiler = filterTranspiler;
            _queryTranspiler = queryTranspiler;
            _snippetsTranspiler = snippetsTranspiler;
        }

        /// <summary>
        /// Преобразует запрос для поиска документов по префиксам слов в Elastic Query DSL
        /// </summary>
        public JObject Transpile(SuggestRequest request)
        {
            var query = new JObject
            {
                ["track_total_hits"] = true,
            };

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
                query["highlight"] = _snippetsTranspiler.BuildSuggestSnippets(request.Snippets);
            }
            if (request.Where != null)
            {
                var boolQuery = new JObject
                {
                    ["filter"] = _filterTranspiler.BuildWhere(request.Where),
                };

                if (!String.IsNullOrWhiteSpace(request.Query))
                {
                    boolQuery["must"] = _queryTranspiler.BuildSuggestQuery(
                        request.Query, request.RequiredWordsCount, request.Weights);
                }

                query["query"] = Bool(boolQuery);
            }
            else if (!String.IsNullOrWhiteSpace(request.Query))
            {
                query["query"] = _queryTranspiler.BuildSuggestQuery(
                    request.Query, request.RequiredWordsCount, request.Weights);
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