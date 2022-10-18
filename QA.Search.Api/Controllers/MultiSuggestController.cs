using Elasticsearch.Net;
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Поиск документов по префиксам слов в текстовых полях: мультиплексирование запросов
    /// </summary>
    [Route("api/v1/multi_suggest")]
    [Consumes("application/json")]
    public class MultiSuggestController : ElasticController
    {
        private readonly SuggestTranspiler _suggestTranspiler;
        private readonly SuggestMapper _suggestMapper;

        public MultiSuggestController(
            IOptions<Settings> options,
            ILogger<MultiSuggestController> logger,
            IElasticLowLevelClient elastic,
            IndexTranspiler indexTranspiler,
            SuggestTranspiler suggestTranspiler,
            SuggestMapper suggestMapper)
            : base(options, logger, elastic, indexTranspiler)
        {
            _suggestTranspiler = suggestTranspiler;
            _suggestMapper = suggestMapper;
        }

        /// <summary>
        /// Поиск документов по префиксам слов в текстовых полях: мультиплексирование запросов
        /// </summary>
        /// <response code="207">Наборы найденных документов</response>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost]
        [ProducesResponseType(typeof(MultiSuggestResponse[]), 207)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> MultiSuggest([FromBody, MaxLength(50)] JObject[] queries)
        {
            JsonSchema schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/suggest.json");

            var apiResponses = new object[queries.Length];

            var validQueries = new List<JObject>(queries.Length);
            var suggestRequests = new List<SuggestRequest>(queries.Length);

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
                    suggestRequests.Add(SuggestRequest.FromJson(queries[i], i));
                }
            }
            if (validQueries.Count == 0)
            {
                return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
            }

            _logger.LogInformation("@{@ApiRequest}", validQueries.ConvertAll(query => query.ToString(Formatting.None)));

            return await FetchResults(apiResponses, suggestRequests);
        }

        private async Task<IActionResult> FetchResults(object[] apiResponses, List<SuggestRequest> suggestRequests)
        {
            List<string> elasticRequests = suggestRequests.ConvertAll(TranspileSuggestRequest);

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
                    apiResponses[suggestRequests[i].ArrayIndex] = _suggestMapper
                        .MapSuggestResponse(responseBody, suggestRequests[i]);
                }
                else
                {
                    apiResponses[suggestRequests[i].ArrayIndex] = MapElasticError(responseBody);
                }
            }

            return StatusCode((int)HttpStatusCode.MultiStatus, apiResponses);
        }

        private string TranspileSuggestRequest(SuggestRequest suggestRequest)
        {
            string indexesWildcard = _indexTranspiler.IndexesWildcard(suggestRequest.From);

            JObject elasticRequest = _suggestTranspiler.Transpile(suggestRequest);

            return SerializeElasticRequest(indexesWildcard, elasticRequest);
        }
    }
}