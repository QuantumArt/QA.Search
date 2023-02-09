using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Interfaces;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.Integration.Core.Helpers;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Services;
using QA.Search.Generic.Integration.QP.Permissions.Configuration;
using QA.Search.Generic.Integration.QP.Permissions.Markers;
using QA.Search.Generic.Integration.QP.Permissions.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Permissions.Services
{
    public class ScheduledServiceQPPermissions : ScheduledServiceBase<IndexingQpPermissionsContext, Settings<QpPermissionsMarker>, QpPermissionsMarker>
    {
        private readonly IOptions<PermissionsConfiguration> _permConfig;
        private readonly PermissionsLoader _permissionsLoader;
        private readonly ScheduledServiceSynchronization _synchronization;

        public ScheduledServiceQPPermissions(
            IOptions<Settings<QpPermissionsMarker>> settingsOptions,
            IndexingQpPermissionsContext context,
            ILogger<QpPermissionsMarker> logger,
            IElasticLowLevelClient elasticClient,
            IOptions<ContextConfiguration> contextConfigurationOption,
            IOptions<PermissionsConfiguration> permConfig,
            PermissionsLoader permissionsLoader,
            IElasticSettingsProvider elasticSettingsProvider,
            ScheduledServiceSynchronization synchronization)
            : base(settingsOptions, context, logger, elasticClient, null, elasticSettingsProvider, contextConfigurationOption)
        {
            _permConfig = permConfig;
            _permissionsLoader = permissionsLoader;

            _context.Reports.Clear();
            _context.Reports.Add(new IndexingReport(_permConfig.Value.PermissionIndexName, 1000));
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
                _context.Message = "Индексация разрешений пользователей.";

                _logger.LogTrace("Start permissions indexing process.");

                foreach (var report in _context.Reports)
                {
                    Stopwatch loaded = Stopwatch.StartNew();
                    List<IndexesByRoles> permissions = await _permissionsLoader.LoadAsync(stoppingToken);
                    loaded.Stop();
                    report.DocumentsLoadTime = loaded.Elapsed;

                    Stopwatch processed = Stopwatch.StartNew();
                    JObject[] documents = new JObject[permissions.Count];

                    for (int elementNumber = 0; elementNumber < permissions.Count; elementNumber++)
                    {
                        IndexesByRoles rolePermissions = permissions[elementNumber];
                        documents[elementNumber] = JObject.FromObject(rolePermissions);
                    }

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
                            jw.WriteValue(document["Role"]);
                            jw.WriteEndObject();
                            jw.WriteEndObject();

                            sw.Write('\n');

                            document.WriteTo(jw);
                            // we shoudn't write '\n' to the end of string
                            // because PostData.MultiJson() already do this
                            results.Add(sw.ToString());
                        }
                    }

                    PostData postData = PostData.MultiJson(results);
                    processed.Stop();
                    report.DocumentsProcessTime = processed.Elapsed;

                    Stopwatch uploaded = Stopwatch.StartNew();
                    string index = GetIndexName(report.IndexName, _context);
                    StringResponse? bulkResponse = await _elastic.BulkAsync<StringResponse>(index, postData, null, stoppingToken);
                    uploaded.Stop();
                    report.DocumentsIndexTime = uploaded.Elapsed;

                    HanldeBulkResponse(bulkResponse, report, documents);

                    _context.Message = "Переключение на новую версию индексов";
                    await UpdateAliases();

                    _context.Message = "Индексация разрешений пользователей завершена";
                }
            }
            finally
            {
                if (isSemaphoreAcquired)
                {
                    _synchronization.Semaphore.Release();
                }
            }
        }
    }
}
