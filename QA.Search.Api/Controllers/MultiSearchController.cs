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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск: мультиплексирование запросов
    /// </summary>
    [Route("api/v1/multi_search")]
    [Consumes("application/json")]
    public class MultiSearchController : ElasticController
    {
        private readonly SearchTranspiler _searchTranspiler;
        private readonly SearchMapper _searchMapper;
        private readonly CorrectionTranspiler _correctionTranspiler;
        private readonly CorrectionMapper _correctionMapper;
        private readonly IndexMapper _indexMapper;
        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public MultiSearchController(
            IOptions<Settings> options,
            ILogger<MultiSearchController> logger,
            IElasticLowLevelClient elastic,
            SearchTranspiler searchTranspiler,
            SearchMapper searchMapper,
            CorrectionTranspiler correctionTranspiler,
            CorrectionMapper correctionMapper,
            IndexMapper indexMapper,
            IElasticSettingsProvider elasticSettingsProvider)
            : base(options, logger, elastic)
        {
            _searchTranspiler = searchTranspiler;
            _searchMapper = searchMapper;
            _correctionTranspiler = correctionTranspiler;
            _correctionMapper = correctionMapper;
            _indexMapper = indexMapper;
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        /// <summary>
        /// Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск: мультиплексирование запросов
        /// </summary>
        /// <response code="207">Наборы найденных документов</response>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost]
        [ProducesResponseType(typeof(MultiSearchResponse[]), 207)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> MultiSearch([FromBody, MaxLength(50)] JObject[] queries)
        {
            JsonSchema schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/search.json");

            var apiResponses = new object[queries.Length];

            var validQueries = new List<JObject>(queries.Length);
            var searchRequests = new List<SearchRequest>(queries.Length);

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
                    searchRequests.Add(SearchRequest.FromJson(queries[i], i));
                }
            }
            if (validQueries.Count == 0)
            {
                return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
            }

            _logger.LogInformation("@{@ApiRequest}", validQueries.ConvertAll(query => query.ToString(Formatting.None)));

            return await FetchResults(apiResponses, searchRequests);
        }

        /// <remarks>
        /// 1. Ищем документы
        /// 2. При необходимости исправляем запросы
        /// 3. При необходимости повторяем поиск документов по исправленным запросам
        /// </remarks>
        private async Task<IActionResult> FetchResults(object[] apiResponses, List<SearchRequest> searchRequests)
        {
            List<string> elasticRequests = searchRequests.ConvertAll(TranspileSearchRequest);

            var msearchResponse = await MsearchAsync(elasticRequests);

            if (!msearchResponse.Success)
            {
                return ElasticError(msearchResponse);
            }

            JObject[] elasticResponses = JObject.Parse(msearchResponse.Body)["responses"].Cast<JObject>().ToArray();

            int successfulRequestCount = 0;
            int queryCorrectionCount = 0;

            for (int i = 0; i < elasticResponses.Length; i++)
            {
                JObject responseBody = elasticResponses[i];

                int status = (int)responseBody["status"];

                if (status == (int)HttpStatusCode.OK)
                {
                    SearchResponse searchResponse = _searchMapper
                        .MapSearchResponse(responseBody, searchRequests[i]);

                    apiResponses[searchRequests[i].ArrayIndex] = searchResponse;

                    successfulRequestCount++;

                    if (ShouldCorrectQuery(searchRequests[i], searchResponse))
                    {
                        queryCorrectionCount++;
                    }
                }
                else
                {
                    apiResponses[searchRequests[i].ArrayIndex] = MapElasticError(responseBody);
                }
            }

            if (queryCorrectionCount < successfulRequestCount)
            {
                return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
            }

            return await CorrectQueries(apiResponses, searchRequests);
        }

        /// <remarks>
        /// Ищем исправления отдельно по каждому индексу. Т.к. если Elastic не надет исправлений
        /// хотябы в одном индексе из перечисленных в wildcard — то он те выдаст результаты вообще.
        /// </remarks>
        private async Task<IActionResult> CorrectQueries(object[] apiResponses, List<SearchRequest> searchRequests)
        {
            string[][] indexNameGroups = await Task.WhenAll(searchRequests.Select(request => GetIndexNames(request)));

            var elasticRequests = new List<string>();

            for (int i = 0; i < searchRequests.Count; i++)
            {
                SearchRequest searchRequest = searchRequests[i];
                string[] indexNames = indexNameGroups[i];

                if (indexNames != null)
                {
                    JObject elasticRequest = _correctionTranspiler.Transpile(searchRequest);

                    foreach (string indexName in indexNames)
                    {
                        elasticRequests.Add(SerializeElasticRequest(indexName, elasticRequest));
                    }
                }
            }

            var msearchCorrectionResponse = await MsearchAsync(elasticRequests);

            if (!msearchCorrectionResponse.Success)
            {
                return ElasticError(msearchCorrectionResponse);
            }

            JObject[] elasticResponses = JObject.Parse(msearchCorrectionResponse.Body)["responses"].Cast<JObject>().ToArray();

            var resultQueryCorrections = new List<QueryCorrection>();
            var resultCorrectionRequests = new List<SearchRequest>();

            int respIndex = 0;
            for (int reqIndex = 0; reqIndex < searchRequests.Count; reqIndex++)
            {
                SearchRequest searchRequest = searchRequests[reqIndex];
                string[] indexNames = indexNameGroups[reqIndex];

                // can be null in case of /_cat/aliases failure
                if (indexNames == null)
                {
                    continue;
                }

                JObject[] responsesSlice = new JObject[indexNames.Length];
                Array.Copy(elasticResponses, respIndex, responsesSlice, 0, indexNames.Length);

                var queryCorrection = _correctionMapper.MapQueryCorrection(responsesSlice);

                var searchResponse = (SearchResponse)apiResponses[searchRequest.ArrayIndex];

                searchResponse.QueryCorrection = queryCorrection;

                if (ShouldCorrectResults(searchRequest, searchResponse))
                {
                    resultQueryCorrections.Add(queryCorrection);
                    resultCorrectionRequests.Add(searchRequest);
                }

                respIndex += indexNames.Length;
            }

            if (resultCorrectionRequests.Count == 0)
            {
                return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
            }

            return await CorrectResults(apiResponses, resultCorrectionRequests, resultQueryCorrections);
        }

        /// <remarks>
        /// Если имена индексов сдержат wildcard или знак исключения,
        /// то эти имена нужно загрузить из Elastic отдельным запросом
        /// </remarks>
        private async Task<string[]> GetIndexNames(SearchRequest searchRequest)
        {
            string indexesWildcard = _elasticSettingsProvider.GetIndexesWildcard(searchRequest.From);

            string[] indexNames = indexesWildcard.Split(',');

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
                    return null;
                }

                indexNames = _indexMapper.AliasesFromCatResponse(JArray.Parse(aliasesResponse.Body));
            }

            return indexNames;
        }

        /// <remarks>
        /// Повторяем поиск по скорректированным запросам
        /// </remarks>
        private async Task<IActionResult> CorrectResults(
            object[] apiResponses, List<SearchRequest> searchRequests, List<QueryCorrection> queryCorrections)
        {
            for (int i = 0; i < searchRequests.Count; i++)
            {
                searchRequests[i].SetQuery(queryCorrections[i].Text);
            }

            List<string> elasticRequests = searchRequests.ConvertAll(TranspileSearchRequest);

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
                    SearchResponse searchResponse = _searchMapper
                        .MapSearchResponse(responseBody, searchRequests[i]);

                    searchResponse.QueryCorrection = queryCorrections[i];

                    queryCorrections[i].ResultsAreCorrected = true;

                    apiResponses[searchRequests[i].ArrayIndex] = searchResponse;
                }
            }

            return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
        }

        private string TranspileSearchRequest(SearchRequest searchRequest)
        {
            string indexesWildcard = _elasticSettingsProvider.GetIndexesWildcard(searchRequest.From);

            JObject elasticRequest = _searchTranspiler.Transpile(searchRequest);

            return SerializeElasticRequest(indexesWildcard, elasticRequest);
        }
    }
}