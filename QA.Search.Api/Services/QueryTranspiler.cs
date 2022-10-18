using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QA.Search.Api.Services
{
    public class QueryTranspiler
    {
        /// <summary>
        /// Преобразует поисковый ввод <paramref name="text"/> запроса /api/search
        ///  в Query DSL "multi_match": "most_fields" по полям, указанным в <paramref name="weights"/>
        /// </summary>
        public JObject BuildSearchQuery(string text, JValue requiredWordsCount, WeightsExpression weights)
        {
            var fields = new List<string>();

            if (weights != null)
            {
                foreach (var (name, weight) in weights)
                {
                    fields.Add($"{name}.synonyms^{weight}");
                    fields.Add($"{name}.shilgles^{weight}");
                }
            }

            fields.Add("_synonyms");
            fields.Add("_shilgles");

            var multiMatch = new JObject
            {
                ["query"] = PrepareText(text),
                ["type"] = "best_fields",
                ["fields"] = new JArray(fields.ToArray())
            };

            if (requiredWordsCount != null)
            {
                multiMatch["minimum_should_match"] = requiredWordsCount;
            }
            else
            {
                multiMatch["operator"] = "and";
            }

            return new JObject
            {
                ["multi_match"] = multiMatch,
            };
        }

        /// <summary>
        /// Преобразует поисковый ввод <paramref name="text"/> запроса /api/suggest
        ///  в Query DSL "multi_match": "best_fields" по полям, указанным в <paramref name="weights"/>
        /// </summary>
        public JObject BuildSuggestQuery(string text, JValue requiredWordsCount, WeightsExpression weights)
        {
            var fields = new List<string>();

            if (weights == null)
            {
                fields.Add("_prefixes");
            }
            else
            {
                foreach (var (name, weight) in weights)
                {
                    fields.Add($"{name}.prefixes^{weight}");
                }
            }

            var multiMatch = new JObject
            {
                ["query"] = PrepareText(text),
                ["type"] = "best_fields",
                ["fields"] = new JArray(fields.ToArray())
            };

            if (requiredWordsCount != null)
            {
                multiMatch["minimum_should_match"] = requiredWordsCount;
            }
            else
            {
                multiMatch["operator"] = "and";
            }

            return new JObject
            {
                ["multi_match"] = multiMatch,
            };
        }

        /// <summary>
        /// Преобразует поисковый ввод <paramref name="text"/> запроса /api/completion
        /// в Query DSL "match" по полю "_prefixes"
        /// </summary>
        /// <remarks>
        /// Не используем веса для увеличения скорости поиска
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-search-speed.html#_search_as_few_fields_as_possible
        /// </remarks>
        public JObject BuildCompletionQuery(string text)
        {
            return new JObject
            {
                ["match"] = new JObject
                {
                    ["_prefixes"] = new JObject
                    {
                        ["query"] = PrepareText(text),
                        ["operator"] = "and",
                    }
                }
            };
        }

        public string PrepareText(string text)
        {
            return WhiteSpace.Replace(text.Trim().ToLowerInvariant(), " ");
        }

        private static readonly Regex WhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);
    }
}