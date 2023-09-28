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
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Services
{
    /// <summary>
    /// Сервис полной индексации необходимых контентов БД.
    /// </summary>
    public class ScheduledServiceQP : ScheduledServiceBase<IndexingQpContext, Settings<QpMarker>, QpMarker>
    {
        private readonly ElasticView<GenericDataContext>[] _views;
        private readonly ViewOptions _viewOptions;
        private readonly ContextConfiguration _contextConfiguration;
        private readonly ScheduledServiceSynchronization _synchronization;

        public ScheduledServiceQP(
            IOptions<Settings<QpMarker>> settingsOptions,
            IndexingQpContext context,
            ILogger<QpMarker> logger,
            IElasticLowLevelClient elasticClient,
            DocumentMiddleware<QpMarker> documentMiddleware,
            IEnumerable<ElasticView<GenericDataContext>> views,
            IOptions<ViewOptions> viewOptions,
            IOptions<ContextConfiguration> contextConfigurationOption,
            IElasticSettingsProvider elasticSettingsProvider,
            ScheduledServiceSynchronization synchronization)
           : base(settingsOptions, context, logger, elasticClient, documentMiddleware, elasticSettingsProvider, contextConfigurationOption)
        {
            _viewOptions = viewOptions.Value;
            _contextConfiguration = contextConfigurationOption.Value;

            _context.Reports.Clear();

            List<ElasticView<GenericDataContext>> enabledViews = new();

            foreach (ElasticView<GenericDataContext> view in views)
            {
                int batchSize = _viewOptions.DefaultBatchSize;

                //Maybe throw error?...
                if (_viewOptions.ViewParameters.TryGetValue(view.ViewName, out ViewParameters viewParameters))
                {
                    if (viewParameters.BatchSize > 0)
                    {
                        batchSize = viewParameters.BatchSize;
                        _logger.LogWarning($"Set default BatchSize = {batchSize}");
                    }

                    if (viewParameters.IsDisabled)
                        _logger.LogInformation("View {ViewName} was disable", view.ViewName);
                    else
                        enabledViews.Add(view);
                }

                _context.Reports.Add(new IndexingReport(view.IndexName, batchSize));
            }

            _synchronization = synchronization;
            _views = enabledViews.ToArray();
        }

        protected override async Task ExecuteStepAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _synchronization.Semaphore.WaitAsync(stoppingToken);

                _context.Reports.ForEach(report => report.Clean());
                _context.Message = "Индексация QP";

                for (int i = 0; i < _views.Length; i++)
                {
                    _context.Reports[i].IdsLoaded = await _views[i].CountAsync(SqlDateTime.MinValue.Value, stoppingToken);
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

                    await view.InitAsync(SqlDateTime.MinValue.Value, stoppingToken);

                    _context.Message = $"Индексация QP: {view.IndexName}";

                    while (true)
                    {
                        loadWatch.Start();
                        LoadParameters loadParameters = new LoadParameters()
                        {
                            FromID = fromId,
                            FromDate = SqlDateTime.MinValue.Value,
                            ViewParameters = _viewOptions.ViewParameters[view.ViewName]
                        };

                        documents = await view.LoadAsync(loadParameters, stoppingToken);
                        loadWatch.Stop();

                        if (documents.Length == 0) break;

                        // получаем Id последней загруженной статьи,
                        // чтобы начать следующую партию JSON-документов с него
                        fromId = documents.Last().GetArticleId(_contextConfiguration);
                        report.DocumentsLoadTime = loadWatch.Elapsed;

                        _logger.LogDebug($"Load documents QP elapsed: {loadWatch.Elapsed}");

                        await IndexDocuments(view, report, documents, processWatch, indexWatch, stoppingToken);

                        handledCount += documents.Length;
                        _context.Progress = handledCount * 100 / totalCount;
                    }
                }

                _context.Message = "Переключение на новую версию индексов";
                await UpdateAliases();

                _context.Message = "Индексация QP завершена";
            }
            finally
            {
                _synchronization.Semaphore.Release();
            }
        }

        private async Task IndexDocuments(
            ElasticView<GenericDataContext> view,
            IndexingReport report,
            JObject[] documents,
            Stopwatch processWatch,
            Stopwatch indexWatch,
            CancellationToken stoppingToken)
        {
            documents = documents.Where(document => document.ContainsKey("SearchUrl")).ToArray();

            if (documents.Length == 0) return;

            string index = GetIndexName(view.IndexName, _context);

            processWatch.Start();
            PostData body = PostData.MultiJson(await SerializeDocuments(documents, stoppingToken));
            processWatch.Stop();
            report.DocumentsProcessTime = processWatch.Elapsed;

            _logger.LogDebug($"Process documents QP elapsed: {processWatch.Elapsed}");

            indexWatch.Start();
            var bulkResponse = await _elastic.BulkAsync<StringResponse>(index, body, null, stoppingToken);
            indexWatch.Stop();
            report.DocumentsIndexTime = indexWatch.Elapsed;

            _logger.LogDebug($"Indexing documents QP elapsed: {indexWatch.Elapsed}");

            HanldeBulkResponse(bulkResponse, report, documents);
        }
    }
}