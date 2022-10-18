using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class SuggestMapper
    {
        private readonly IndexMapper _indexMapper;
        private readonly SnippetsMapper _snippetsMapper;
        private readonly ContextualFieldsMapper _contextualFieldsMapper;

        public SuggestMapper(
            IndexMapper indexMapper,
            SnippetsMapper snippetsMapper,
            ContextualFieldsMapper contextualFieldsMapper)
        {
            _indexMapper = indexMapper;
            _snippetsMapper = snippetsMapper;
            _contextualFieldsMapper = contextualFieldsMapper;
        }

        /// <summary>
        /// Извлекает набор найденных документов из ответа Elastic /_search. Вычисляет контекстные поля.
        /// </summary>
        public SuggestResponse MapSuggestResponse(JObject body, SuggestRequest suggestRequest)
        {
            var hitsObject = (JObject)body["hits"];
            var hitsArray = (JArray)hitsObject["hits"];
            var aggregations = body["aggregations"] as JObject;

            FilterExpression contextFilter = suggestRequest.Context ?? suggestRequest.Where;

            _contextualFieldsMapper.TransformContextualFields(hitsArray, contextFilter);

            return new SuggestResponse
            {
                Documents = hitsArray.Cast<JObject>().Select(MapSuggestDocument).ToArray(),
            };
        }

        /// <summary>
        /// Извлекает набор найденных документов из агрегированного ответа Elastic /_search. Вычисляет контекстные поля.
        /// </summary>
        public SuggestResponse MapSuggestAggrResponse(JObject body)
        {
            var aggregations = body[Common.Settings.ElasticSearch.AggregationsFieldName] as JObject;
            var suggestions =
                aggregations
                ?[Common.Settings.ElasticSearch.SuggestionsFieldName]
                ?[Common.Settings.ElasticSearch.BucketsFieldName] as JArray;

            return new SuggestResponse
            {
                Documents = suggestions.Cast<JObject>().Select(x => new ElasticDocument { AdditionalData = x }).ToArray(),
            };
        }

        private ElasticDocument MapSuggestDocument(JObject body) => new ElasticDocument
        {
            Id = (string)body["_id"],
            Index = _indexMapper.ShortIndexName((string)body["_index"]),
            Score = (float?)body["_score"] ?? 0,
            Snippets = _snippetsMapper.MapSuggestSnippets(body["highlight"]),
            AdditionalData = (JObject)body["_source"],
        };
    }
}