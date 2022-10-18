using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class SnippetsMapper
    {
        /// <summary>
        /// Преобразовать ответ от Elastic в словарь подсвеченных фрагментов текста,
        /// найденных с использованием шинглов
        /// </summary>
        public Dictionary<string, string[]> MapSearchSnippets(JToken highlight) => MapSnippets(highlight, "shingles");

        /// <summary>
        /// Преобразовать ответ от Elastic в словарь подсвеченных фрагментов текста,
        /// найденных с использованием префиксов
        /// </summary>
        public Dictionary<string, string[]> MapSuggestSnippets(JToken highlight) => MapSnippets(highlight, "prefixes");
        
        /// <example>
        /// {
        ///   "Title": ["В <b>Москве</b> открыли"],
        ///   "Regions.Title": ["<b>Москва</b> и область"]
        /// }
        /// </example>
        private Dictionary<string, string[]> MapSnippets(JToken highlight, string fieldSuffix)
        {
            string specialField = $"_{fieldSuffix}";
            int extraLength = fieldSuffix.Length + 1;

            var highlightObject = (IDictionary<string, JToken>)highlight;

            return highlightObject?.ToDictionary(
                pair => pair.Key == specialField ? "_all" : pair.Key.Substring(0, pair.Key.Length - extraLength),
                pair => pair.Value.Select(token => (string)token).ToArray());
        }
    }
}