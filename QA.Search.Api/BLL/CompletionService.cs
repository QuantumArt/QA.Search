using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Search.Api.Builders;
using QA.Search.Api.Models;
using QA.Search.Api.Models.ElasticSearch;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QA.Search.Api.BLL
{
    public class CompletionService
    {
        private readonly Settings _settings;
        private readonly IElasticLowLevelClient _elastic;
        private readonly ILogger _logger;

        public CompletionService(
            IOptions<Settings> settingsOptions,
            ILogger<CompletionService> logger,
            IElasticLowLevelClient elastic)
        {
            _settings = settingsOptions.Value;
            _logger = logger;
            _elastic = elastic;
        }

        /// <summary>
        /// Регистрация пользовательского запроса
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<StringResponse> RegisterUserQuery(SuggestRegisterRequest request)
        {
            var index = SetIndexPrefix(_settings.UserQueryIndex);
            var newObj = new UserQueryIndex
            {
                Query = request.Query,
                Region = request.Region
            };
            var builder = new EsRequestBuilder();
            builder.SetInsertedObj(newObj);

            return await InsertRowAsync(index, builder.Build());
        }

        /// <summary>
        /// Получение рекомендаций пользовательских запросов
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<StringResponse> GetUserQuerySuggestion(QuerySuggestionRequest request)
        {
            var mmQueryBuilder = new MultiMatchSectionBuilder();
            mmQueryBuilder.SetQuery(request.Query);

            var mmRegionBuilder = new MultiMatchSectionBuilder();
            mmRegionBuilder.SetQuery(request.Region);
            mmRegionBuilder.AddField("region");

            var queryBuilder = new QuerySectionBuilder();
            queryBuilder.AddMultiMatches(mmQueryBuilder.Build(), mmRegionBuilder.Build());

            var aggsBuilder = new AggsSectionBuilder();
            aggsBuilder.SetAggrName(Common.Settings.ElasticSearch.SuggestionsFieldName);
            aggsBuilder.SetTermsField("query");
            aggsBuilder.SetTermsSize(request.Limit > 0 ? request.Limit.Value : _settings.SuggestionsDefaultLength);

            var suggestionBuilder = new EsRequestBuilder();
            suggestionBuilder.SetSize(0);
            suggestionBuilder.SetQuerySection(queryBuilder.Build());
            suggestionBuilder.SetAggsSection(aggsBuilder.Build());

            var index = SetIndexPrefix(_settings.UserQueryIndex);
            return await SearchAsync(index, suggestionBuilder.Build());
        }

        private async Task<StringResponse> SearchAsync(string indexesWildcard, string elasticRequest)
        {
            return await ExecuteAsync(x => x.SearchAsync<StringResponse>(indexesWildcard, elasticRequest));
        }

        private Task<StringResponse> InsertRowAsync(string indexName, string elasticRequest)
        {
            return ExecuteAsync(x => x.IndexAsync<StringResponse>(indexName, Common.Settings.ElasticSearch.DocType, elasticRequest));
        }

        private async Task<StringResponse> ExecuteAsync(Func<IElasticLowLevelClient, Task<StringResponse>> func)
        {
            var sw = Stopwatch.StartNew();
            var response = await func(_elastic);
            sw.Stop();

            _logger.LogTrace("@{@ElasticRequest}", response);
            _logger.LogInformation("Request to {ElasticUri} executed in {Msec} msec", response.Uri, sw.ElapsedMilliseconds);
            return response;
        }

        private string SetIndexPrefix(string name)
        {
            return string.Concat(_settings.IndexPrefix, name);
        }
    }
}
