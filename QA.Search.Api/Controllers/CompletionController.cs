using Elasticsearch.Net;
using Elasticsearch.Net.Specification.CatApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Api.BLL;
using QA.Search.Api.Infrastructure;
using QA.Search.Api.Models;
using QA.Search.Api.Services;
using System.Net;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Дополнение строки поискового ввода
    /// </summary>
    [Route("api/v1/completion")]
    [Consumes("application/json")]
    public class CompletionController : ElasticController
    {
        private readonly CompletionTranspiler _completionTranspiler;
        private readonly CompletionMapper _completionMapper;
        private readonly IndexMapper _indexMapper;
        private readonly CompletionService _elasticSearchService;
        private readonly SuggestMapper _suggestMapper;

        public CompletionController(
            IOptions<Settings> options,
            ILogger<CompletionController> logger,
            IElasticLowLevelClient elastic,
            IndexTranspiler indexTranspiler,
            CompletionTranspiler completionTranspiler,
            CompletionMapper completionMapper,
            IndexMapper indexMapper,
            CompletionService elasticSearchService,
            SuggestMapper suggestsMapper)
            : base(options, logger, elastic, indexTranspiler)
        {
            _completionTranspiler = completionTranspiler;
            _completionMapper = completionMapper;
            _indexMapper = indexMapper;
            _elasticSearchService = elasticSearchService;
            _suggestMapper = suggestsMapper;
        }

        /// <summary>
        /// Дополнение строки поискового ввода
        /// </summary>
        /// <returns>Предложения поисковых фраз</returns>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="404">Индекс не найден</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost]
        [ProducesResponseType(typeof(CompletionResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> Completion([FromBody] JObject query)
        {
            var schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/completion.json");

            if (!schema.TryValidateModel(query, out var problemDetails))
            {
                return BadRequest(problemDetails);
            }

            _logger.LogInformation("@{@ApiRequest}", query.ToString(Formatting.None));

            CompletionRequest completionRequest = CompletionRequest.FromJson(query);

            if (_settings.UsePermissions)
            {
                string[] from = await GetIndexesToSearchFromPermissionsAsync(completionRequest.Roles);

                if (from is null || from.Length == 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                completionRequest.SetFrom(from);
            }

            // для /{index}/_analyze нужен какой-то валидный индекс Elastic,
            // в котором зарегистрирован "analyzer_regex"
            string analyzeIndexName = _indexTranspiler.IndexNameForAnalyze(completionRequest.From);

            if (analyzeIndexName == null)
            {
                var aliasesResponse = await _elastic.Cat.AliasesAsync<StringResponse>(
                    _settings.AliasMask, new CatAliasesRequestParameters
                    {
                        Headers = new[] { "alias" },
                        Format = "json",
                        Local = true,
                    });

                if (!aliasesResponse.Success)
                {
                    return ElasticError(aliasesResponse);
                }

                analyzeIndexName = _indexMapper.FirstAliasFromCatResponse(JArray.Parse(aliasesResponse.Body));
            }

            string analyzeRequest = _completionTranspiler
                .TranspileAnalyze(completionRequest)
                .ToString(Formatting.None);

            var analyzeResponse = await _elastic.Indices.AnalyzeAsync<StringResponse>(analyzeIndexName, analyzeRequest);

            if (!analyzeResponse.Success)
            {
                return ElasticError(analyzeResponse);
            }

            string[] tokens = _completionMapper.MapAnalyzeResponse(JObject.Parse(analyzeResponse.Body));

            completionRequest.SetTokens(tokens);

            string indexesWildcard = _indexTranspiler.IndexesWildcard(completionRequest.From);

            string elasticRequest = _completionTranspiler
                .TranspileCompletion(completionRequest)
                .ToString(Formatting.None);

            var elasticResponse = await SearchAsync(indexesWildcard, elasticRequest);

            if (!elasticResponse.Success)
            {
                return ElasticError(elasticResponse);
            }

            var apiResponse = _completionMapper.MapCompletionResponse(JObject.Parse(elasticResponse.Body));

            return Ok(apiResponse);
        }

        /// <summary>
        /// Поиск предположений по префиксам слов в текстовых полях
        /// </summary>
        /// <returns>Набор найденных документов</returns>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="404">Индекс не найден</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost("query")]
        [ProducesResponseType(typeof(SuggestResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> QuerySuggestions([FromBody] QuerySuggestionRequest request)
        {
            var response = await _elasticSearchService.GetUserQuerySuggestion(request);
            if (!response.Success)
            {
                return ElasticError(response);
            }
            var apiResponse = _suggestMapper
                .MapSuggestAggrResponse(JObject.Parse(response.Body));
            return Ok(apiResponse);
        }

        /// <summary>
        /// Регистрация запроса для вывода часто используемых запросов
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost("query_register")]
        [ProducesResponseType(typeof(SearchResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> RegisterQueryCompletion([FromBody] SuggestRegisterRequest request)
        {
            var response = await _elasticSearchService.RegisterUserQuery(request);
            if (!response.Success)
            {
                return ElasticError(response);
            }
            return Ok();
        }
    }
}