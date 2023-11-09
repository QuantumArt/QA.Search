using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class CorrectionTranspiler
    {
        private readonly FilterTranspiler _filterTranspiler;
        private readonly QueryTranspiler _queryTranspiler;

        public CorrectionTranspiler(
            FilterTranspiler filterTranspiler,
            QueryTranspiler queryTranspiler)
        {
            _filterTranspiler = filterTranspiler;
            _queryTranspiler = queryTranspiler;
        }

        /// <summary>
        /// Преобразует запрос к /api/search в синтаксис Elastic "suggest".
        /// Используется "phrase" suggester по полю "_phrases", с "direct_generator"
        /// по полям из <see cref="SearchRequest.Weights"/> (при наличии).
        /// После этого происходит фильтрация предложенных исправлений по
        /// поисковому вводу <see cref="SearchRequest.Query"/> с помощью опции "collate".
        /// </summary>
        /// <remarks>
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/search-suggesters-phrase.html
        /// </remarks>
        public JObject Transpile(SearchRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Query))
            {
                throw new ArgumentException("Query is empty", nameof(request));
            }

            var suggest = new JObject
            {
                ["size"] = 1,
                ["highlight"] = new JObject
                {
                    ["pre_tag"] = "<b>",
                    ["post_tag"] = "</b>",
                }
            };

            PopulateCorrectionFields(suggest, request);

            PopulateCollationQuery(suggest, request);

            return new JObject
            {
                ["size"] = 0,
                ["track_total_hits"] = true,
                ["suggest"] = new JObject
                {
                    ["text"] = _queryTranspiler.PrepareText(request.Query),
                    ["query_correction"] = new JObject
                    {
                        ["phrase"] = suggest,
                    },
                },
            };
        }

        private void PopulateCorrectionFields(JObject suggest, SearchRequest request)
        {
            if (request.Weights == null)
            {
                suggest["field"] = "_phrases";
                suggest["direct_generator"] = new JArray
                {
                    new JObject
                    {
                        ["field"] = "_phrases",
                        ["suggest_mode"] = "popular",
                        ["min_word_length"] = 3,
                    }
                };
            }
            else
            {
                if (request.Weights.Count() == 1)
                {
                    string field = request.Weights.Single().Key;
                    suggest["field"] = $"{field}.phrases";
                }
                else
                {
                    suggest["field"] = "_phrases";
                }

                JObject[] directGenerator = request.Weights
                    .Select(weight => new JObject
                    {
                        ["field"] = $"{weight.Key}.phrases",
                        ["suggest_mode"] = "popular",
                        ["min_word_length"] = 3,
                    })
                    .ToArray();

                suggest["direct_generator"] = new JArray(directGenerator);
            }
        }

        /// <remarks>
        /// Не используем веса для увеличения скорости поиска
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-search-speed.html#_search_as_few_fields_as_possible
        /// </remarks>
        private void PopulateCollationQuery(JObject suggest, SearchRequest request)
        {
            JObject query;

            if (request.Where != null)
            {
                query = Bool(new JObject
                {
                    ["filter"] = _filterTranspiler.BuildWhere(request.Where),
                    ["must"] = _queryTranspiler.BuildSearchQuery(
                        "{{suggestion}}", request.RequiredWordsCount, weights: null),
                });
            }
            else
            {
                query = _queryTranspiler.BuildSearchQuery(
                    "{{suggestion}}", request.RequiredWordsCount, weights: null);
            }

            suggest["collate"] = new JObject
            {
                ["query"] = new JObject
                {
                    ["source"] = query,
                }
            };
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