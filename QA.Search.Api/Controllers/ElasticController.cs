using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using QA.Search.Api.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Базовый контроллер для запросов в Elastic
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public abstract class ElasticController : Controller
    {
        protected readonly Settings _settings;
        protected readonly ILogger _logger;
        protected readonly IElasticLowLevelClient _elastic;
        protected readonly IndexTranspiler _indexTranspiler;

        protected ElasticController(
            IOptions<Settings> options,
            ILogger logger,
            IElasticLowLevelClient elastic,
            IndexTranspiler indexTranspiler)
        {
            _settings = options.Value;
            _logger = logger;
            _elastic = elastic;
            _indexTranspiler = indexTranspiler;
        }

        protected ObjectResult ElasticError(StringResponse response)
        {
            int statusCode = response.HttpStatusCode ?? (int)HttpStatusCode.BadGateway;

            var executionProplem = new ProblemDetails
            {
                Status = statusCode,
                Title = response.OriginalException.Message,
            };

            if (response.TryGetServerError(out var exception))
            {
                executionProplem.Type = exception.Error.Type;
                executionProplem.Detail = exception.Error.Reason;
            }

            _logger.LogWarning("[{StatusCode}] {ElasticUri} {Message}",
                statusCode, response.Uri, response.OriginalException.Message);

            if (statusCode != (int)HttpStatusCode.BadGateway)
            {
                _logger.LogWarning("@{@ElasticRequest}", Encoding.UTF8.GetString(response.RequestBodyInBytes));
            }

            return StatusCode(statusCode, executionProplem);
        }

        protected ProblemDetails MapElasticError(JObject response)
        {
            _logger.LogWarning($"Elastic error {response}");

            JObject rootCause = response["error"]?["root_cause"]?.First as JObject;

            string type = (string)rootCause?["type"];
            string reason = (string)rootCause["reason"];

            return new ProblemDetails
            {
                Status = (int)response["status"],
                Type = type,
                Title = reason,
                Detail = reason,
            };
        }

        protected async Task<StringResponse> SearchAsync(string indexesWildcard, string elasticRequest)
        {
            _logger.LogTrace("@{@ElasticRequest} {indexesWildcard}", elasticRequest, indexesWildcard);

            var sw = new Stopwatch();
            sw.Start();

            var elasticResponse = await _elastic.SearchAsync<StringResponse>(indexesWildcard, elasticRequest);
            sw.Stop();

            if (elasticResponse.Success)
            {
                _logger.LogInformation("Request to {ElasticUri} executed in {Msec} msec",
                   elasticResponse.Uri, sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("@{ElasticUri}{Msec}{DebugInformation}",
                   elasticResponse.Uri, sw.ElapsedMilliseconds, elasticResponse.DebugInformation);
            }


            return elasticResponse;
        }

        protected async Task<StringResponse> MsearchAsync(IEnumerable<string> elasticRequests)
        {
            _logger.LogTrace("@{@ElasticRequests}", elasticRequests);

            var sw = new Stopwatch();
            sw.Start();

            var elasticResponse = await _elastic.MultiSearchAsync<StringResponse>(PostData.MultiJson(elasticRequests));
            sw.Stop();

            _logger.LogInformation("Request to {ElasticUri} executed in {Msec} msec",
                elasticResponse.Uri, sw.ElapsedMilliseconds);

            return elasticResponse;
        }

        /// <summary>
        /// Сериализовать один запрос для /_msearch
        /// </summary>
        protected string SerializeElasticRequest(string indexName, JObject elasticRequest)
        {
            using (var sw = new StringWriter())
            using (var jw = new JsonTextWriter(sw))
            {
                jw.WriteStartObject();
                jw.WritePropertyName("index");
                jw.WriteValue(indexName);
                jw.WritePropertyName("type");
                jw.WriteValue("_doc");
                jw.WriteEndObject();

                sw.Write('\n');

                elasticRequest.WriteTo(jw);
                // we shoudn't write '\n' to the end of string
                // because PostData.MultiJson() already do this
                return sw.ToString();
            }
        }

        /// <summary>
        /// Нужно скорректировать строку поиска, если нашлось не более, чем
        /// <code>$correct.$query.$ifFoundLte</code> или не более, чем
        /// <code>$correct.$result.$ifFoundLte</code> документов.
        /// </summary>
        protected bool ShouldCorrectQuery(SearchRequest searchRequest, SearchResponse searchResponse)
        {
            _logger.LogDebug($"{nameof(searchRequest)} = {searchRequest}; {nameof(searchResponse)} = {searchResponse}");

            if (String.IsNullOrWhiteSpace(searchRequest.Query) || searchRequest.Offset > 0)
            {
                return false;
            }

            int queryLimit = searchRequest.Correct?.Query?.IfFoundLte ?? -1;

            int resultsLimit = searchRequest.Correct?.Results?.IfFoundLte ?? -1;

            _logger.LogDebug($"{nameof(queryLimit)} = {queryLimit}; {nameof(resultsLimit)} = {resultsLimit}");

            return searchResponse.TotalCount <= queryLimit || searchResponse.TotalCount <= resultsLimit;
        }

        /// <summary>
        /// Нужно скорректировать результаты поиска по скорректированной строке,
        /// если нашлось не более, чем <code>$correct.$results.$ifFoundLte</code> документов.
        /// </summary>
        protected bool ShouldCorrectResults(SearchRequest searchRequest, SearchResponse searchResponse)
        {
            if (!ShouldCorrectQuery(searchRequest, searchResponse) || searchResponse.QueryCorrection == null)
            {
                return false;
            }

            int resultsLimit = searchRequest.Correct?.Results?.IfFoundLte ?? -1;

            return searchResponse.TotalCount <= resultsLimit;
        }

        /// <summary>
        /// Достаёт список индексов доступных для ролей полученных в запросе из индекса указанного в конфиге.
        /// </summary>
        /// <param name="roles">Список ролей из запроса.</param>
        /// <returns>Список индексов в которых резрешено искать.</returns>
        protected async Task<string[]> GetIndexesToSearchFromPermissionsAsync(string[] roles)
        {
            if (roles is null || roles.Length == 0)
            {
                roles = new string[] { _settings.DefaultReaderRole };
            }
            else
            {
                if (!roles.Contains(_settings.DefaultReaderRole))
                {
                    Array.Resize(ref roles, roles.Length + 1);
                    roles[^1] = _settings.DefaultReaderRole;
                }
            }

            RolePermissionsRequest rolesRequest = new RolePermissionsRequest();
            rolesRequest.Query.Subquery.Roles = roles;
            string query = JsonConvert.SerializeObject(rolesRequest);

            if (string.IsNullOrWhiteSpace(_settings.PermissionsIndexName))
            {
                _logger.LogError("Parameter [{PermissionsIndexName}] with permissions index name is empty!", nameof(_settings.PermissionsIndexName));
                return null;
            }

            StringResponse rolesSearchResponse = await SearchAsync(_settings.PermissionsIndexName, query);

            if (!rolesSearchResponse.Success)
            {
                _logger.LogError(rolesSearchResponse.OriginalException, "Error while reading permissions index.");
                return null;
            }

            JObject result = JObject.Parse(rolesSearchResponse.Body);
            var hitsObject = (JObject)result["hits"];

            if (hitsObject is null)
            {
                _logger.LogWarning("Can't find hits object in elastic result.");
                return null;
            }

            var hitsArray = (JArray)hitsObject["hits"];

            if (hitsArray is null)
            {
                _logger.LogInformation("There is no permissions for roles.");
                return null;
            }

            var sources = hitsArray.Select(x => x["_source"]).ToArray();

            if (sources is null || sources.Length == 0)
            {
                _logger.LogWarning("Can't find data in hits.");
                return null;
            }

            var indexes = sources.Select(x => x["Indexes"]).ToArray().SelectMany(x => x).Distinct().ToArray();

            if (indexes is null || indexes.Length == 0)
            {
                _logger.LogWarning("Can't extract indexes tokens from sources.");
                return null;
            }

            var indexArray = indexes.Select(x => x.ToObject<string>()).ToArray();

            if (indexArray is null || indexArray.Length == 0)
            {
                _logger.LogWarning("Can't convert indexes to string array.");
                return null;
            }

            return indexArray;
        }
    }
}