using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Api.Infrastructure;
using QA.Search.Api.Models;
using QA.Search.Api.Services;
using QA.Search.Common.Interfaces;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Поиск документов по префиксам слов в текстовых полях
    /// </summary>
    [Route("api/v1/suggest")]
    [Consumes("application/json")]
    public class SuggestController : ElasticController
    {
        private readonly SuggestTranspiler _suggestTranspiler;
        private readonly SuggestMapper _suggestMapper;
        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public SuggestController(
            IOptions<Settings> options,
            ILogger<SuggestController> logger,
            IElasticLowLevelClient elastic,
            SuggestTranspiler suggestTranspiler,
            SuggestMapper suggestsMapper,
            IElasticSettingsProvider elasticSettingsProvider)
            : base(options, logger, elastic)
        {
            _suggestTranspiler = suggestTranspiler;
            _suggestMapper = suggestsMapper;
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        /// <summary>
        /// Поиск документов по префиксам слов в текстовых полях
        /// </summary>
        /// <returns>Набор найденных документов</returns>
        /// <response code="400">Невалидный запрос</response>
        /// <response code="404">Индекс не найден</response>
        /// <response code="408">Таймаут при ожидании ответа от Elastic</response>
        /// <response code="502">ElasticSearch не доступен</response>
        [HttpPost]
        [ProducesResponseType(typeof(SuggestResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 408)]
        [ProducesResponseType(typeof(ProblemDetails), 502)]
        public async Task<IActionResult> Suggest([FromBody] JObject query)
        {
            var schema = await JsonSchemaRegistry.GetSchema("JsonSchemas/suggest.json");

            if (!schema.TryValidateModel(query, out var problemDetails))
            {
                return BadRequest(problemDetails);
            }

            _logger.LogInformation("@{@ApiRequest}", query.ToString(Formatting.None));

            SuggestRequest suggestRequest = SuggestRequest.FromJson(query);
            await suggestRequest.SetPresetsAsync();

            if (_settings.UsePermissions)
            {
                string[] from = await GetIndexesToSearchFromPermissionsAsync(suggestRequest.Roles);

                if (from is null || from.Length == 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                suggestRequest.SetFrom(from);
            }

            string indexesWildcard = _elasticSettingsProvider.GetIndexesWildcard(suggestRequest.From);

            string elasticRequest = _suggestTranspiler.Transpile(suggestRequest).ToString(Formatting.None);

            var elasticResponse = await SearchAsync(indexesWildcard, elasticRequest);

            if (!elasticResponse.Success)
            {
                return ElasticError(elasticResponse);
            }

            var apiResponse = _suggestMapper
                .MapSuggestResponse(JObject.Parse(elasticResponse.Body), suggestRequest);

            return Ok(apiResponse);
        }
    }
}