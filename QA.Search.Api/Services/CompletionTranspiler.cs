using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QA.Search.Api.Services
{
    public class CompletionTranspiler
    {
        private readonly FilterTranspiler _filterTranspiler;
        private readonly QueryTranspiler _queryTranspiler;

        public CompletionTranspiler(FilterTranspiler filterTranspiler, QueryTranspiler queryTranspiler)
        {
            _filterTranspiler = filterTranspiler;
            _queryTranspiler = queryTranspiler;
        }

        /// <summary>
        /// Преобразует поисковую строку в запрос к ElasticSearch /_analyze
        /// </summary>
        public JObject TranspileAnalyze(CompletionRequest request)
        {
            return new JObject
            {
                ["analyzer"] = "analyzer_regex",
                ["text"] = _queryTranspiler.PrepareText(request.Query),
            };
        }

        /// <summary>
        /// Преобразует запрос на дополнение строки поискового ввода в Elastic агрегацию "significant_text",
        /// применяемую после фильтрации докуметнов по поисковой строке $query и условиям, заданным в блоке $where
        /// </summary>
        public JObject TranspileCompletion(CompletionRequest request)
        {
            var query = new JObject
            {
                ["size"] = 0,
                ["track_total_hits"] = false,
            };

            if (request.Where != null)
            {
                var boolQuery = new JObject
                {
                    ["filter"] = _filterTranspiler.BuildWhere(request.Where),
                };

                if (!String.IsNullOrWhiteSpace(request.Query))
                {
                    boolQuery["must"] = _queryTranspiler.BuildCompletionQuery(request.Query);
                }

                query["query"] = Bool(boolQuery);
            }
            else if (!String.IsNullOrWhiteSpace(request.Query))
            {
                query["query"] = _queryTranspiler.BuildCompletionQuery(request.Query);
            }

            query["aggregations"] = new JObject
            {
                ["query_completion"] = new JObject
                {
                    ["significant_text"] = BuildSignificantText(request),
                }
            };

            return query;
        }

        private static JObject Bool(JToken boolQuery)
        {
            return new JObject
            {
                ["bool"] = boolQuery,
            };
        }

        /// <summary>
        /// Построение агрегации "significant_text" по виртуальным полям, которые содержат шинглы из 5 слов.
        /// Результаты агрегации (поисковые фразы) дополнительно фильтруются регулярным выражением,
        /// построенным из токенов исходной строки поиска. Токены получаются с помощью вызова Elastic /_analyze.
        /// </summary>
        private static JObject BuildSignificantText(CompletionRequest request)
        {
            if (request.Weights == null || request.Weights.Count() == 0)
            {
                throw new ArgumentException("Weights can not be empty", nameof(request));
            }
            if (request.Tokens == null)
            {
                throw new ArgumentException("Tokens can not be null", nameof(request));
            }

            var result = new JObject
            {
                ["min_doc_count"] = 1,
            };
            
            if (request.Limit != null)
            {
                result["size"] = request.Limit;
            }
            if (request.Weights.Count() == 1)
            {
                string field = request.Weights.Single().Key;
                result["field"] = $"{field}.phrases";
            }
            else
            {
                string[] sourceFields = request.Weights
                    .Select(weight => weight.Key)
                    .ToArray();

                result["field"] = "_phrases";
                result["source_fields"] = new JArray(sourceFields);
            }

            string filterRegex = BuildFilterRegex(request.Tokens);

            if (!String.IsNullOrEmpty(filterRegex))
            {
                result["include"] = filterRegex;
            }

            return result;
        }

        /// <summary>
        /// Построение регулярного выражения: содержит все токены <paramref name="tokens"/> в любом порядке
        /// </summary>
        private static string BuildFilterRegex(string[] tokens)
        {
            if (tokens.Length == 0)
            {
                return null;
            }
            if (tokens.Length == 1)
            {
                return $@"{Escape(tokens[0])}.*";
            }

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-regexp-query.html
            // '&' in ElasticSearch regexp syntax means INTERSECTION of multiple conditions
            // phrase: "меняются условия услуг"
            // tokens: ["меня", "услов", "услуг"]
            // regex: (.* )?меня.*&(.* )?услов.*&(.* )?услуг.*

            return String.Join('&', tokens.Select(token => $@"(.* )?{Escape(token)}.*"));
        }

        private static string Escape(string token)
        {
            return ElasticRegexSymbols.Replace(Regex.Escape(token), @"\$0");
        }

        private static Regex ElasticRegexSymbols = new Regex(@"[#@&<>~]", RegexOptions.Compiled);
    }
}