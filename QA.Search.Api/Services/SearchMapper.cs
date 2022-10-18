using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class SearchMapper
    {
        private readonly IndexMapper _indexMapper;
        private readonly SnippetsMapper _snippetsMapper;
        private readonly FacetsMapper _facetsMapper;
        private readonly ContextualFieldsMapper _contextualFieldsMapper;

        public SearchMapper(
            IndexMapper indexMapper,
            SnippetsMapper snippetsMapper,
            FacetsMapper facetsMapper,
            ContextualFieldsMapper contextualFieldsMapper)
        {
            _indexMapper = indexMapper;
            _snippetsMapper = snippetsMapper;
            _facetsMapper = facetsMapper;
            _contextualFieldsMapper = contextualFieldsMapper;
        }

        /// <summary>
        /// Извлекает набор найденных документов и словарь фасетов
        /// из ответа Elastic /_search. Вычисляет контекстные поля.
        /// </summary>
        public SearchResponse MapSearchResponse(JObject body, SearchRequest searchRequest)
        {
            var hitsObject = (JObject)body["hits"];
            var hitsArray = (JArray)hitsObject["hits"];

            FilterExpression contextFilter = searchRequest.Context ?? searchRequest.Where;

            _contextualFieldsMapper.TransformContextualFields(hitsArray, contextFilter);

            return new SearchResponse
            {
                TotalCount = (int)hitsObject["total"]["value"],
                Documents = hitsArray.Cast<JObject>().Select(MapSearchDocument).ToArray(),
                Facets = body["aggregations"] is JObject aggregations ? _facetsMapper.MapFacets(aggregations) : null,
            };
        }

        private ElasticDocument MapSearchDocument(JObject body) => new ElasticDocument
        {
            Id = (string)body["_id"],
            Index = _indexMapper.ShortIndexName((string)body["_index"]),
            Score = (float?)body["_score"] ?? 0,
            Snippets = _snippetsMapper.MapSearchSnippets(body["highlight"]),
            AdditionalData = (JObject)body["_source"],
        };
    }
}