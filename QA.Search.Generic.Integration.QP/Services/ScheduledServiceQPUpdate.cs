using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Interfaces;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.Integration.Core.Helpers;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Models.DTO;
using QA.Search.Generic.Integration.Core.Processors;
using QA.Search.Generic.Integration.Core.Services;
using QA.Search.Generic.Integration.QP.Extensions;
using QA.Search.Generic.Integration.QP.Infrastructure;
using QA.Search.Generic.Integration.QP.Markers;
using QA.Search.Generic.Integration.QP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Services
{


    /// <summary>
    /// Сервис частичной индексации контентов БД. Обновляет (без пересоздания индекса) только те документы,
    /// корневая статья которых была изменена за прожедшие два календарных дня (сегодня и вчера).
    /// </summary>
    public class ScheduledServiceQPUpdate : ScheduledServiceBase<IndexingQpUpdateContext, Settings<QpUpdateMarker>, QpUpdateMarker>
    {
        private readonly ElasticView<GenericDataContext>[] _views;
        private readonly ViewOptions _viewOptions;
        private readonly ContextConfiguration _contextConfiguration;
        private readonly ScheduledServiceSynchronization _synchronization;

        public ScheduledServiceQPUpdate(
            IOptions<Settings<QpUpdateMarker>> settingsOptions,
            IndexingQpUpdateContext context,
            ILogger<QpUpdateMarker> logger,
            IElasticLowLevelClient elasticClient,
            DocumentMiddleware<QpUpdateMarker> documentMiddleware,
            IEnumerable<ElasticView<GenericDataContext>> views,
            IOptions<ViewOptions> viewOptions,
            IOptions<ContextConfiguration> contextConfigurationOption,
            IElasticSettingsProvider elasticSettingsProvider,
            ScheduledServiceSynchronization synchronization)
           : base(settingsOptions, context, logger, elasticClient, documentMiddleware, elasticSettingsProvider, contextConfigurationOption)
        {
            _views = views.ToArray();
            _viewOptions = viewOptions.Value;
            _contextConfiguration = contextConfigurationOption.Value;

            _context.Reports.Clear();

            for (int viewIndex = 0; viewIndex < _views.Length; viewIndex++)
            {
                int batchSize = _viewOptions.DefaultBatchSize;

                //Maybe throw error?...
                if (_viewOptions.ViewParameters.TryGetValue(_views[viewIndex].ViewName, out ViewParameters viewParameters))
                    if (viewParameters.BatchSize > 0)
                    {
                        batchSize = viewParameters.BatchSize;
                        _logger.LogWarning($"Set default BatchSize = {batchSize}");
                    }

                _context.Reports.Add(new IndexingReport(_views[viewIndex].IndexName, batchSize));
            }
            _synchronization = synchronization;
        }

        protected override async Task ExecuteStepAsync(CancellationToken stoppingToken)
        {
            bool isSemaphoreAcquired = false;

            try
            {
                isSemaphoreAcquired = await _synchronization.Semaphore.WaitAsync(_synchronization.SemaphoreAwaitTime, stoppingToken);

                if (!isSemaphoreAcquired)
                {
                    _context.Message = _synchronization.SemaphoreBusyMessage;
                    return;
                }

                _context.Reports.ForEach(report => report.Clean());
                _context.Message = "Обновление QP";

                // обновляем в Elastic только документы, корневая статья которых была изменена за прошедшие 2 дня
                DateTime fromDate = DateTime.Today.AddDays(-1);

                for (int i = 0; i < _views.Length; i++)
                {
                    _context.Reports[i].IdsLoaded = await _views[i].CountAsync(fromDate, stoppingToken);
                }

                int totalCount = _context.Reports.Sum(report => report.IdsLoaded);
                int handledCount = 0;

                for (int i = 0; i < _views.Length; i++)
                {
                    int fromId = 0;
                    JObject[] documents;
                    var view = _views[i];
                    var report = _context.Reports[i];
                    var loadWatch = new Stopwatch();
                    var processWatch = new Stopwatch();
                    var indexWatch = new Stopwatch();

                    _context.Message = $"Подготовка QP: {view.IndexName}";

                    await view.InitAsync(fromDate, stoppingToken);

                    _context.Message = $"Обновление QP: {view.IndexName}";

                    while (true)
                    {
                        loadWatch.Start();
                        LoadParameters loadParameters = new LoadParameters()
                        {
                            FromID = fromId,
                            FromDate = fromDate,
                            ViewParameters = _viewOptions.ViewParameters[view.ViewName]
                        };

                        documents = await view.LoadAsync(loadParameters, stoppingToken);
                        loadWatch.Stop();

                        if (documents.Length == 0) break;

                        // получаем Id последней загруженной статьи,
                        // чтобы начать следующую партию JSON-документов с него
                        fromId = documents.Last().GetArticleId(_contextConfiguration);
                        report.DocumentsLoadTime = loadWatch.Elapsed;

                        _logger.LogDebug($"Load documents QPUpdate elapsed: {loadWatch.Elapsed}");

                        await UpdateDocuments(view, report, documents, processWatch, indexWatch, stoppingToken);

                        handledCount += documents.Length;
                        _context.Progress = handledCount * 100 / totalCount;
                    }
                }

                _context.Message = "Обновление QP завершено";
            }
            finally
            {
                if (isSemaphoreAcquired)
                {
                    _synchronization.Semaphore.Release();
                }
            }
        }

        private async Task UpdateDocuments(
            ElasticView<GenericDataContext> view,
            IndexingReport report,
            JObject[] documents,
            Stopwatch processWatch,
            Stopwatch indexWatch,
            CancellationToken stoppingToken)
        {
            documents = documents.Where(document => document.ContainsKey("SearchUrl")).ToArray();

            if (documents.Length == 0) return;

            string alias = GetAliasName(view.IndexName);

            var aliasExistsResponse = await _elastic.Indices.AliasExistsForAllAsync<StringResponse>(alias, ctx: stoppingToken);

            if (!aliasExistsResponse.Success || aliasExistsResponse.HttpStatusCode != 200) return;

            processWatch.Start();
            PostData body = PostData.MultiJson(await SerializeDocuments(documents, stoppingToken));
            processWatch.Stop();
            report.DocumentsProcessTime = processWatch.Elapsed;

            _logger.LogDebug($"Process documents QPUpdate elapsed: {processWatch.Elapsed}");

            indexWatch.Start();
            var bulkResponse = await _elastic.BulkAsync<StringResponse>(alias, body, null, stoppingToken);
            indexWatch.Stop();
            report.DocumentsIndexTime = indexWatch.Elapsed;

            _logger.LogDebug($"Indexing documents QPUpdate elapsed: {indexWatch.Elapsed}");

            HanldeBulkResponse(bulkResponse, report, documents);
        }
    }
}