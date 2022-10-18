using Elasticsearch.Net;
using Elasticsearch.Net.Specification.CatApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using QA.Search.Api.Infrastructure;
using QA.Search.Api.Models;
using QA.Search.Api.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск
    /// </summary>
    [Route("api/v1/search")]
    [Consumes("application/json")]
    public class SearchController : ElasticController
    {
        private readonly SearchTranspiler _searchTranspiler;
        private readonly SearchMapper _searchMapper;
        private readonly CorrectionTranspiler _correctionTranspiler;
        private readonly CorrectionMapper _correctionMapper;
        private readonly IndexMapper _indexMapper;

        public SearchController(
            IOptions<Settings> options,
            ILogger<SearchController> logger,
            IElasticLowLevelClient elastic,
            IndexTranspiler indexTranspiler,
            SearchTranspiler searchTranspiler,
            SearchMapper searchMapper,
            CorrectionTranspiler correctionTranspiler,
            CorrectionMapper correctionMapper,
            IndexMapper indexMapper)
            : base(options, logger, elastic, indexTranspiler)
        {
            _searchTranspiler = searchTranspiler;
            _searchMapper = searchMapper;
            _correctionTranspiler = correctionTranspiler;
            _correctionMapper = correctionMapper;
            _indexMapper = indexMapper;
        }

        /// <summary>
        /// Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск
        /// </summary>
        /// <returns>Набор найденных документов</returns>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="404">Индекс не найден</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost("obsolete")]
        [ProducesResponseType(typeof(SearchResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> SearchObsolete([FromBody] JObject query)
        {
            JsonSchema schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/search.json");

            if (!schema.TryValidateModel(query, out var validationProblem))
            {
                return ValidationProblem(validationProblem);
            }

            _logger.LogInformation("@{@ApiRequest}", query.ToString(Formatting.None));

            SearchRequest searchRequest = SearchRequest.FromJson(query);

            return await FetchResults(searchRequest);
        }

        /// <summary>
        /// Полнотекстовый поиск документов с пресетами, с синонимами и морфологией, фасетный поиск
        /// </summary>
        /// <returns>Набор найденных документов</returns>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="404">Индекс не найден</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost]
        [ProducesResponseType(typeof(SearchResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> Search([FromBody] JObject query)
        {
            JsonSchema schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/search.json");

            if (!schema.TryValidateModel(query, out var validationProblem))
            {
                return ValidationProblem(validationProblem);
            }

            _logger.LogInformation("@{@ApiRequest}", query.ToString(Formatting.None));

            Stopwatch stopwatch = Stopwatch.StartNew();

            SearchRequest searchRequest = SearchRequest.FromJson(query);
            await searchRequest.SetPresetsAsync();

            stopwatch.Stop();

            _logger.LogTrace($"Searh SetPresetsAsync elapsed {stopwatch.Elapsed}");

            return await FetchResults(searchRequest);
        }

        /// <remarks>
        /// 1. Ищем документы
        /// 2. При необходимости исправляем запрос
        /// 3. При необходимости повторяем поиск документов по исправленному запросу
        /// </remarks>
        private async Task<IActionResult> FetchResults(SearchRequest searchRequest)
        {
            if (_settings.UsePermissions)
            {
                string[] from = await GetIndexesToSearchFromPermissionsAsync(searchRequest.Roles);

                if (from is null || from.Length == 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                searchRequest.SetFrom(from);
            }

            string indexesWildcard = _indexTranspiler.IndexesWildcard(searchRequest.From);

            string elasticRequest = _searchTranspiler.Transpile(searchRequest).ToString(Formatting.None);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var elasticResponse = await SearchAsync(indexesWildcard, elasticRequest);

            stopwatch.Stop();

            _logger.LogTrace($"Searh FetchResults.SearchAsync elapsed {stopwatch.Elapsed}");

            if (!elasticResponse.Success)
            {
                return ElasticError(elasticResponse);
            }

            var searchResponse = _searchMapper
                .MapSearchResponse(JObject.Parse(elasticResponse.Body), searchRequest);

            if (!ShouldCorrectQuery(searchRequest, searchResponse))
            {
                return Ok(searchResponse);
            }

            return await CorrectQuery(searchRequest, searchResponse);
        }

        /// <remarks>
        /// Ищем исправления отдельно по каждому индексу. Т.к. если Elastic не надет исправлений
        /// хотябы в одном индексе из перечисленных в wildcard — то он те выдаст результаты вообще.
        /// </remarks>
        private async Task<IActionResult> CorrectQuery(SearchRequest searchRequest, SearchResponse searchResponse)
        {
            string indexesWildcard = _indexTranspiler.IndexesWildcard(searchRequest.From);

            List<string> indexNames = indexesWildcard.Split(',').ToList();

            // Если имена индексов сдержат wildcard или знак исключения,
            // то эти имена нужно загрузить из Elastic отдельным запросом
            if (indexesWildcard.Contains('*') || indexesWildcard.StartsWith('-') || indexesWildcard.Contains(",-"))
            {
                var aliasesResponse = await _elastic.Cat.AliasesAsync<StringResponse>(
                   indexesWildcard, new CatAliasesRequestParameters
                   {
                       Headers = new[] { "alias" },
                       Format = "json",
                       Local = true,
                   });

                if (!aliasesResponse.Success)
                {
                    return ElasticError(aliasesResponse);
                }

                indexNames = _indexMapper.AliasesFromCatResponse(JArray.Parse(aliasesResponse.Body)).ToList();
            }

            JObject correctionRequest = _correctionTranspiler.Transpile(searchRequest);

            List<string> elasticRequests = indexNames
                .ConvertAll(indexName => SerializeElasticRequest(indexName, correctionRequest));

            Stopwatch stopwatch = Stopwatch.StartNew();

            var correctionResponse = await MsearchAsync(elasticRequests);

            stopwatch.Stop();

            _logger.LogTrace($"Searh CorrectQuery.MsearchAsync elapsed {stopwatch.Elapsed}");

            if (!correctionResponse.Success)
            {
                return ElasticError(correctionResponse);
            }

            JObject[] elasticResponses = JObject.Parse(correctionResponse.Body)["responses"].Cast<JObject>().ToArray();

            QueryCorrection queryCorrection = _correctionMapper.MapQueryCorrection(elasticResponses);

            searchResponse.QueryCorrection = queryCorrection;

            if (!ShouldCorrectResults(searchRequest, searchResponse))
            {
                return Ok(searchResponse);
            }

            return await CorrectResults(searchRequest, searchResponse, queryCorrection);
        }

        /// <remarks>
        /// Повторяем поиск по скорректированному запросу
        /// </remarks>
        private async Task<IActionResult> CorrectResults(
            SearchRequest searchRequest, SearchResponse searchResponse, QueryCorrection queryCorrection)
        {
            searchRequest.SetQuery(queryCorrection.Text);

            string indexesWildcard = _indexTranspiler.IndexesWildcard(searchRequest.From);

            var elasticRequest = _searchTranspiler.Transpile(searchRequest).ToString(Formatting.None);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var elasticResponse = await SearchAsync(indexesWildcard, elasticRequest);

            stopwatch.Stop();

            _logger.LogTrace($"Searh CorrectResults.SearchAsync elapsed {stopwatch.Elapsed}");


            if (!elasticResponse.Success)
            {
                return ElasticError(elasticResponse);
            }

            searchResponse = _searchMapper
                .MapSearchResponse(JObject.Parse(elasticResponse.Body), searchRequest);

            searchResponse.QueryCorrection = queryCorrection;

            queryCorrection.ResultsAreCorrected = true;

            return Ok(searchResponse);
        }
    }
}