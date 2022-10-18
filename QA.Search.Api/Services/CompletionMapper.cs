using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class CompletionMapper
    {
        /// <summary>
        /// Извлекает массив токенов из ответа Elastic /_analyze
        /// </summary>
        public string[] MapAnalyzeResponse(JObject body)
        {
            var tokensArray = (JArray)body["tokens"];

            return tokensArray
                .Cast<JObject>()
                .Select(tokenObject => (string)tokenObject["token"])
                .ToArray();
        }

        /// <summary>
        /// Извлекает массив поисковых фраз из результата агрегации "significant_text"
        /// </summary>
        public CompletionResponse MapCompletionResponse(JObject body)
        {
            var aggregationsObject = (JObject)body["aggregations"];
            var queryCompletionObject = (JObject)aggregationsObject["query_completion"];
            var bucketsArray = (JArray)queryCompletionObject["buckets"];

            string[] phrases = bucketsArray
                .Cast<JObject>()
                .Select(item => (string)item["key"])
                .ToArray();

            return new CompletionResponse
            {
                Phrases = phrases,
            };
        }
    }
}