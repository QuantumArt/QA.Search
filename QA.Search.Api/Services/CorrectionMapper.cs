using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System.Linq;
using System.Net;

namespace QA.Search.Api.Services
{
    public class CorrectionMapper
    {
        /// <summary>
        /// Извлечь исправленную поисковую строку в текстовом виде
        /// и с HTML-выделением исправленных фраз из ответа Elastic /_msearch
        /// </summary>
        public QueryCorrection MapQueryCorrection(JObject[] responses)
        {
            return responses
                .Select(response => MapQueryCorrection(response))
                .Where(correction => correction != null)
                .OrderByDescending(correction => correction.Score)
                .FirstOrDefault();
        }

        /// <summary>
        /// Извлечь исправленную поисковую строку в текстовом виде
        /// и с HTML-выделением исправленных фраз из ответа Elastic /_search
        /// </summary>
        public QueryCorrection MapQueryCorrection(JObject body)
        {
            int status = (int)body["status"];
            if (status != (int)HttpStatusCode.OK)
            {
                return null;
            }

            var suggestObject = (JObject)body["suggest"];
            if (suggestObject == null)
            {
                return null;
            }

            var queryCorrectionArray = (JArray)suggestObject["query_correction"];
            if (queryCorrectionArray.Count == 0)
            {
                return null;
            }

            var queryCorrectionObject = (JObject)queryCorrectionArray[0];
            var optionsArray = (JArray)queryCorrectionObject["options"];
            if (optionsArray.Count == 0)
            {
                return null;
            }

            var optionObject = (JObject)optionsArray[0];

            return new QueryCorrection
            {
                Score = (float)optionObject["score"],
                Text = (string)optionObject["text"],
                Snippet = (string)optionObject["highlighted"],
            };
        }
    }
}