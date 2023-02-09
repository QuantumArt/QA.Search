using Elasticsearch.Net;
using Elasticsearch.Net.Specification.CatApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using QA.Search.Api.Infrastructure;
using QA.Search.Api.Models;
using QA.Search.Api.Services;
using QA.Search.Common.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Дополнение строки поискового ввода: мультиплексирование запросов
    /// </summary>
    [Route("api/v1/multi_completion")]
    [Consumes("application/json")]
    public class MultiCompletionController : ElasticController
    {
        private readonly CompletionTranspiler _completionTranspiler;
        private readonly CompletionMapper _completionMapper;
        private readonly IndexMapper _indexMapper;
        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public MultiCompletionController(
            IOptions<Settings> options,
            ILogger<MultiCompletionController> logger,
            IElasticLowLevelClient elastic,
            CompletionTranspiler completionTranspiler,
            CompletionMapper completionMapper,
            IndexMapper indexMapper,
            IElasticSettingsProvider elasticSettingsProvider)
            : base(options, logger, elastic)
        {
            _completionTranspiler = completionTranspiler;
            _completionMapper = completionMapper;
            _indexMapper = indexMapper;
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        /// <summary>
        /// Дополнение строки поискового ввода: мультиплексирование запросов
        /// </summary>
        /// <response code="207">Предложения поисковых фраз</response>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost]
        [ProducesResponseType(typeof(MultiCompletionResponse[]), 207)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> MultiCompletion([FromBody, MaxLength(50)] JObject[] queries)
        {
            JsonSchema schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/completion.json");

            var apiResponses = new object[queries.Length];

            var validQueries = new List<JObject>(queries.Length);
            var completionRequests = new List<CompletionRequest>(queries.Length);

            for (int i = 0; i < queries.Length; i++)
            {
                if (!schema.TryValidateModel(queries[i], out var validationProblem))
                {
                    validationProblem.Status = (int)HttpStatusCode.BadRequest;
                    apiResponses[i] = validationProblem;
                }
                else
                {
                    validQueries.Add(queries[i]);
                    completionRequests.Add(CompletionRequest.FromJson(queries[i], i));
                }
            }
            if (validQueries.Count == 0)
            {
                return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
            }

            _logger.LogInformation("@{@ApiRequest}", validQueries.ConvertAll(query => query.ToString(Formatting.None)));

            return await FetchResults(apiResponses, completionRequests);
        }

        private async Task<IActionResult> FetchResults(object[] apiResponses, List<CompletionRequest> completionRequests)
        {
            // для /{index}/_analyze нужен какой-то валидный индекс Elastic,
            // в котором зарегистрирован "analyzer_regex"
            string analyzeIndexName = _elasticSettingsProvider.GetIndexNameForAnalyze(completionRequests.SelectMany(request => request.From));

            if (analyzeIndexName == null)
            {
                var aliasesResponse = await _elastic.Cat.AliasesAsync<StringResponse>(
                    _elasticSettingsProvider.GetAliasMask(), new CatAliasesRequestParameters
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

            await Task.WhenAll(completionRequests
                .Select(request => PopulateRequestTokens(apiResponses, request, analyzeIndexName)));

            // if request.Tokens == null there is an error object in apiResponses already
            completionRequests = completionRequests.Where(request => request.Tokens != null).ToList();

            List<string> elasticRequests = completionRequests.ConvertAll(TranspileCompletionRequest);

            var msearchResponse = await MsearchAsync(elasticRequests);

            if (!msearchResponse.Success)
            {
                return ElasticError(msearchResponse);
            }

            JObject[] elasticResponses = JObject.Parse(msearchResponse.Body)["responses"].Cast<JObject>().ToArray();

            for (int i = 0; i < elasticResponses.Length; i++)
            {
                JObject responseBody = elasticResponses[i];

                int status = (int)responseBody["status"];

                if (status == (int)HttpStatusCode.OK)
                {
                    apiResponses[completionRequests[i].ArrayIndex] = _completionMapper.MapCompletionResponse(responseBody);
                }
                else
                {
                    apiResponses[completionRequests[i].ArrayIndex] = MapElasticError(responseBody);
                }
            }

            return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
        }

        private async Task PopulateRequestTokens(
            object[] apiResponses, CompletionRequest completionRequest, string analyzeIndexName)
        {
            string analyzeRequest = _completionTranspiler
                .TranspileAnalyze(completionRequest)
                .ToString(Formatting.None);

            var analyzeResponse = await _elastic.Indices.AnalyzeAsync<StringResponse>(analyzeIndexName, analyzeRequest);

            JObject responseBody = JObject.Parse(analyzeResponse.Body);

            if (analyzeResponse.Success)
            {
                string[] tokens = _completionMapper.MapAnalyzeResponse(responseBody);

                completionRequest.SetTokens(tokens);
            }
            else
            {
                apiResponses[completionRequest.ArrayIndex] = MapElasticError(responseBody);
            }
        }

        private string TranspileCompletionRequest(CompletionRequest completionRequest)
        {
            string indexesWildcard = _elasticSettingsProvider.GetIndexesWildcard(completionRequest.From);

            JObject elasticRequest = _completionTranspiler.TranspileCompletion(completionRequest);

            return SerializeElasticRequest(indexesWildcard, elasticRequest);
        }
    }
}