using Elasticsearch.Net;
using Elasticsearch.Net.Specification.CatApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Interfaces;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.Integration.Core.Extensions;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.Core.Services
{
    /// <summary>
    /// Base class for Background Workers that can be scheduled by CronTab or can be started manually
    /// </summary>
    public abstract class ScheduledServiceBase<TContext, TSettings, TMarker> : BackgroundService, IServiceController<TMarker>
        where TContext : IndexingContext<TMarker>
        where TSettings : Settings<TMarker>, new()
        where TMarker : IServiceMarker
    {
        protected readonly TSettings _settings;
        protected readonly TContext _context;
        protected readonly ILogger _logger;
        protected readonly IElasticLowLevelClient _elastic;
        protected readonly DocumentMiddleware<TMarker> _documentMiddleware;
        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        private readonly CrontabSchedule _crontabSchedule;

        private CancellationTokenSource _linkedTokenSource;

        private readonly ContextConfiguration _contextConfiguration;

        public ScheduledServiceBase(
            IOptions<TSettings> settingsOptions,
            TContext context,
            ILogger<TMarker> logger,
            IElasticLowLevelClient elasticClient,
            DocumentMiddleware<TMarker> documentMiddleware,
            IElasticSettingsProvider elasticSettingsProvider,
            IOptions<ContextConfiguration> contextConfigurationOption)
        {
            _settings = settingsOptions.Value;
            _logger = logger;
            _context = context;
            _context.Progress = 0;
            _context.State = IndexingState.Stopped;
            _context.ScheduledDates = null;
            _context.StartDate = null;
            _context.EndDate = null;
            _linkedTokenSource = null;
            _elastic = elasticClient;
            _documentMiddleware = documentMiddleware;
            _elasticSettingsProvider = elasticSettingsProvider;

            if (!string.IsNullOrWhiteSpace(_settings.CronSchedule))
            {
                _crontabSchedule = CrontabSchedule.Parse(_settings.CronSchedule);
            }

            _contextConfiguration = contextConfigurationOption.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Start IndexingService");

            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            CancellationToken linkedToken = _linkedTokenSource.Token;

            try
            {
                while (true)
                {
                    try
                    {
                        if (_context.State == IndexingState.AwaitingRun)
                        {
                            _context.State = IndexingState.Running;
                            _context.Message = null;
                            _context.StartDate = DateTime.Now;
                            _context.EndDate = null;
                            _context.Iteration++;

                            try
                            {
                                await ExecuteStepAsync(linkedToken);

                                _context.State = IndexingState.Stopped;
                                _context.EndDate = DateTime.Now;
                                _context.Progress = 0;
                            }
                            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
                            {
                                if (stoppingToken.IsCancellationRequested)
                                {
                                    throw;
                                }
                                else
                                {
                                    _context.State = IndexingState.Stopped;
                                    _context.EndDate = DateTime.Now;
                                    _context.Progress = 0;
                                    _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                                    linkedToken = _linkedTokenSource.Token;
                                }
                            }
                            catch (Exception ex)
                            {
                                _context.State = IndexingState.Error;
                                _context.EndDate = DateTime.Now;
                                _context.Progress = 0;
                                _context.Message = $"Ошибка индексации ({ex.Message})";
                                _logger.LogError(ex, "IndexingService Error on iteration {Iteration}", _context.Iteration);
                            }

                            _logger.LogInformation("End iteration {Iteration}", _context.Iteration);
                        }
                        if (_crontabSchedule != null)
                        {
                            _context.ScheduledDates = _crontabSchedule
                                .GetNextOccurrences(DateTime.Now, DateTime.Now.Date.AddDays(2))
                                .Take(10)
                                .ToArray();

                            DateTime startDate = _crontabSchedule.GetNextOccurrence(DateTime.Now);

                            await Task.Delay(startDate - DateTime.Now, linkedToken);

                            _context.State = IndexingState.AwaitingRun;
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1), linkedToken);
                        }
                    }
                    catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        else
                        {
                            _logger.LogDebug("Custom Cancel {0}", _context.State);
                            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            linkedToken = _linkedTokenSource.Token;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "IndexingService stopped on error");
            }
            finally
            {
                _linkedTokenSource = null;
                _logger.LogInformation("End IndexingService");
            }
        }

        protected abstract Task ExecuteStepAsync(CancellationToken stoppingToken);

        public bool Start()
        {
            if (_linkedTokenSource != null &&
                (_context.State == IndexingState.Stopped || _context.State == IndexingState.Error))
            {
                _context.State = IndexingState.AwaitingRun;
                _linkedTokenSource.Cancel();
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            _logger.LogDebug($"Stop {_context.State}");
            if (_linkedTokenSource != null && _context.State == IndexingState.Running)
            {
                _context.State = IndexingState.AwaitingStop;
                _logger.LogDebug($"Stop Enter {_context.State}");
                _linkedTokenSource.Cancel();
                return true;
            }

            return false;
        }

        protected void HanldeBulkResponse(StringResponse bulkResponse, IndexingReport report, JObject[] documents)
        {
            _logger.LogTrace($"{bulkResponse.DebugInformation}");

            if (bulkResponse.Success)
            {
                var bulkData = JObject.Parse(bulkResponse.Body);

                report.ProductsLoaded += documents.Length;

                if (bulkData.Value<bool>("errors"))
                {
                    var statuses = bulkData.SelectTokens("items[*].index.status");
                    report.ProductsIndexed += statuses.Count(i => i.Value<int>() < 400);

                    var errors = bulkData.SelectTokens("items[*].index")
                        .Where(i => i.Value<int>("status") >= 400)
                        .Select(i => new
                        {
                            Id = i.Value<int>("_id"),
                            Index = i.Value<string>("_index"),
                            Type = i["error"].Value<string>("type"),
                            Message = i["error"].Value<string>("reason"),
                        })
                        .GroupBy(e => new { e.Index, e.Type, e.Message }, e => e.Id)
                        .ToArray();

                    foreach (var errGroup in errors)
                    {
                        _logger.LogWarning(
                            "Index {Index} error [{Type}] {Message} for documents {Ids}",
                            errGroup.Key.Index, errGroup.Key.Type, errGroup.Key.Message, errGroup);
                    }

                    throw new Exception("BulkAsync error on storing data, see logs for details");
                }
                else
                {
                    report.ProductsIndexed += documents.Length;
                }
            }
            else
            {
                _logger.LogError(bulkResponse.OriginalException, $"Elastic BulkAsync error {bulkResponse}");
                throw new Exception("BulkAsync error", bulkResponse.OriginalException);
            }
        }

        /// <summary>
        /// Serialize documents for QP Articles
        /// </summary>
        protected async Task<List<string>> SerializeDocuments(JObject[] documents, CancellationToken stoppingToken)
        {
            documents = await _documentMiddleware.ProcessAsync(documents, stoppingToken);

            var results = new List<string>(documents.Length);

            foreach (JObject document in documents)
            {
                using (var sw = new StringWriter())
                using (var jw = new JsonTextWriter(sw))
                {
                    jw.WriteStartObject();
                    jw.WritePropertyName("index");
                    jw.WriteStartObject();
                    jw.WritePropertyName("_id");
                    jw.WriteValue(document.GetArticleId(_contextConfiguration).ToString());
                    jw.WriteEndObject();
                    jw.WriteEndObject();

                    sw.Write('\n');

                    document.WriteTo(jw);
                    // we shoudn't write '\n' to the end of string
                    // because PostData.MultiJson() already do this
                    results.Add(sw.ToString());
                }
            }

            return results;
        }

        /// <summary>
        /// 1. Перекидываем алиасы со старых индексов на только что созданные и заполненные
        /// 2. Удаляем все старые индексы (одной и той же интеграции, например QP или DPC)
        /// </summary>
        protected async Task UpdateAliases()
        {
            string version = _context.GetDateSuffix();
            string maskToRemove = $"{_elasticSettingsProvider.GetIndexMask()},-{_elasticSettingsProvider.GetIndexMask()}.{version}";
            string maskForAlias = $"{_elasticSettingsProvider.GetIndexMask()}.{version}";
            var query = new { actions = new List<object>() };

            var indicesToRemove = await GetIndices(maskToRemove);

            query.actions.AddRange(indicesToRemove.Select(index => new
            {
                remove_index = new { index }
            }));

            var indicesForAlias = await GetIndices(maskForAlias);

            query.actions.AddRange(indicesForAlias.Select(index => new
            {
                add = new
                {
                    index,
                    alias = GetAliasName(GetDocumentType(index, _context))
                }
            }));

            var json = JsonConvert.SerializeObject(query);

            _logger.LogTrace($"UpdateAliases query = {query}");

            var data = PostData.String(json);

            //TODO handle result
            var updateResponse = await _elastic.Indices.BulkAliasForAllAsync<StringResponse>(data);

            _logger.LogTrace($"IndicesUpdateAliasesForAll response = {updateResponse}");
        }

        /// <summary>
        /// Get all Elastic indices that match passed <paramref name="mask"/>
        /// </summary>
        private async Task<string[]> GetIndices(string mask)
        {
            var indexParams = new CatIndicesRequestParameters { Format = "json", Headers = new[] { "index" } };
            var indexResponse = await _elastic.Cat.IndicesAsync<StringResponse>(mask, indexParams);

            if (indexResponse.Success)
            {
                var indexData = JArray.Parse(indexResponse.Body);
                var indices = indexData.Select(p => p.Value<string>("index")).ToArray();
                return indices;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Get lowercase DPC document type or QP table name, etc. in lowercase by Elastic index name
        /// and indexing context
        /// </summary>
        protected string GetDocumentType(string fullIndexName, IndexingContext<TMarker> context)
        {
            int prefixLength = _elasticSettingsProvider.GetIndexPrefix().Length;
            int suffixLength = $".{context.GetDateSuffix()}".Length;

            return fullIndexName.Substring(prefixLength, fullIndexName.Length - prefixLength - suffixLength);
        }

        /// <summary>
        /// Get full Elastic index name by <paramref name="documentType"/> and indexing start date
        /// that stored in <paramref name="context"/>
        /// </summary>
        protected string GetIndexName(string documentType, IndexingContext<TMarker> context)
        {
            return $"{_elasticSettingsProvider.GetIndexPrefix()}{documentType}.{context.GetDateSuffix()}".ToLower();
        }

        /// <summary>
        /// Get full Elastic Alias name by <paramref name="documentType"/>
        /// </summary>
        protected string GetAliasName(string documentType)
        {
            return $"{_elasticSettingsProvider.GetAliasPrefix()}{documentType}".ToLower();
        }
    }
}