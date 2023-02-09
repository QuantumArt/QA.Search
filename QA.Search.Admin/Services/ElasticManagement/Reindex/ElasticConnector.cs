using Elasticsearch.Net;
using Elasticsearch.Net.Specification.CatApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Admin.Services.ElasticManagement.IndexesInfoParsing;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public partial class ElasticConnector
    {
        #region injected
        private IElasticLowLevelClient Elastic { get; }

        private IndexesInfoParser IndexesParser { get; }

        private ILogger Logger { get; }

        private readonly IElasticSettingsProvider _elasticSettingsProvider;
        #endregion

        public ElasticConnector(IElasticLowLevelClient elastic, IndexesInfoParser indexesParser, IElasticSettingsProvider elasticSettingsProvider, ILogger logger)
        {
            Elastic = elastic;
            IndexesParser = indexesParser;
            _elasticSettingsProvider = elasticSettingsProvider;
            Logger = logger;
        }

        /// <summary>
        /// Получить от Elastic'а список всех индексов
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IElasticIndex>> GetAllIndexes(CancellationToken cancellationToken)
        {
            CatIndicesRequestParameters irp = new CatIndicesRequestParameters
            {
                QueryString = new Dictionary<string, object> {
                    { "index", _elasticSettingsProvider.GetIndexMask() }
                }
            };
            var elasticResponse = await Elastic.Indices.GetAliasAsync<StringResponse>(_elasticSettingsProvider.GetIndexMask());
            if (!elasticResponse.Success)
            {
                Logger.LogWarning(elasticResponse.OriginalException, $"GetAllIndexes - not success response {elasticResponse}");
                throw new ReindexWorkerIterationException("Ошибка при попытке получения списка индексов с алиасами", null);
            }
            var indexesInfo = IndexesParser.ParseIndexes(elasticResponse.Body);
            var res = indexesInfo.Select(i => new ElasticIndex(i));
            return res;
        }

        public async Task CancelTaskIfPossible(IReindexTask task)
        {
            if (!String.IsNullOrWhiteSpace(task?.ElasticTaskId))
            {
                var response = await Elastic.Tasks.CancelAsync<VoidResponse>(task.ElasticTaskId);
                if (!response.Success)
                {
                    Logger.LogError(response.OriginalException, "Ошибка при отмене задачи Elastic {ElasticTaskId}", task.ElasticTaskId);
                }
            }
        }

        public async Task DeleteTaskIfPossible(IReindexTask task)
        {
            if (!String.IsNullOrWhiteSpace(task?.ElasticTaskId))
            {
                var response = await Elastic.Tasks.CancelAsync<VoidResponse>(task.ElasticTaskId);

                if (!response.Success)
                {
                    Logger.LogWarning(response.OriginalException, "Ошибка при удалении задачи Elastic {ElasticTaskId}", task.ElasticTaskId);
                }
            }
        }

        public async Task RemoveIndex(IElasticIndex index)
        {
            if (index == null)
            {
                throw new ArgumentNullException(nameof(index));
            }
            CheckIndexCanBeManaged(index);
            var response = await Elastic.Indices.DeleteAsync<StringResponse>(index.FullName);
            if (!response.Success)
            {
                Logger.LogWarning(response.OriginalException, $"RemoveIndex - not success response {response}");
                throw new InvalidOperationException("Ошибка при попытке удалить индекс", response.OriginalException);
            }
        }

        public async Task RemoveIndex(string indexFullName)
        {

            if (indexFullName == null)
            {
                throw new ArgumentNullException(nameof(indexFullName));
            }
            if (string.IsNullOrWhiteSpace(indexFullName))
            {
                throw new ArgumentException(nameof(indexFullName));
            }
            if (indexFullName == "*" || indexFullName == "_all")
            {
                throw new InvalidOperationException("Данный код не позволяет удалить все индексы");
            }

            var allIndexes = await GetAllIndexes(new CancellationTokenSource().Token);
            var targetIndex = allIndexes.FirstOrDefault(i => i.FullName == indexFullName);
            if (targetIndex == null)
            {
                return;
            }
            await RemoveIndex(targetIndex);
        }

        internal async Task<IElasticIndex> CreateNewIndex(string indexName)
        {
            var indexInfo = IndexesParser.CreateNewIndexInfo(indexName);
            var realIndexName = indexInfo.FullName;
            var realAlias = indexInfo.AliasesWithPrefixes.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(realIndexName) || string.IsNullOrWhiteSpace(realAlias))
            {
                throw new InvalidOperationException();
            }
            var creaeteRes = await Elastic.Indices.CreateAsync<StringResponse>(realIndexName, null);
            if (!creaeteRes.Success)
            {
                throw new InvalidOperationException();
            }

            var paramsObj = new
            {
                actions = new[] {
                    new {
                        add = new {
                            index = realIndexName,
                            alias = realAlias
                        }
                    }
                }
            };


            var body = JsonConvert.SerializeObject(paramsObj);
            var response = await Elastic.Indices.BulkAliasForAllAsync<StringResponse>(PostData.String(body));

            if (!response.Success)
            {
                throw new InvalidOperationException();
            }

            return new ElasticIndex(indexInfo);
        }

        public async Task<CheckTaskStatusResult> GetElasticTaskStatus(IReindexTask task)
        {
            if (string.IsNullOrWhiteSpace(task.ElasticTaskId))
            {
                throw new ArgumentException("У таски нет идентификатора");
            }

            var elResponse = await Elastic.Tasks.GetTaskAsync<StringResponse>(task.ElasticTaskId);

            return new CheckTaskStatusResult(elResponse);
        }


        /// <summary>
        /// Перенести алиас с исходного на целевой индекс
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SwapAliases(IElasticIndex sourceIndex,
            IElasticIndex destinationIndex)
        {
            if (sourceIndex == null)
            {
                throw new ArgumentNullException(nameof(sourceIndex));
            }
            if (destinationIndex == null)
            {
                throw new ArgumentNullException(nameof(destinationIndex));
            }

            Func<string, string, JObject> createIndexAndAliasObj = (index, targetAlias) =>
            {
                return new JObject
                {
                    { "index", index },
                    { "alias", targetAlias }
                };
            };

            var sName = sourceIndex.FullName;
            var dName = destinationIndex.FullName;
            var alias = sourceIndex.AliasWithPrefix;

            if (string.IsNullOrWhiteSpace(sName)
                || string.IsNullOrWhiteSpace(dName)
                || string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            var paramsObj = new
            {
                actions = new object[] {
                    new {
                        remove = new {
                            index = sName,
                            alias = alias
                        }
                    },
                    new {
                        add = new {
                            index = dName,
                            alias = alias
                        }
                    }
                }
            };

            var body = JsonConvert.SerializeObject(paramsObj);
            var response = await Elastic.Indices.BulkAliasForAllAsync<StringResponse>(PostData.String(body));

            if (response.Success)
            {
                var si = (ElasticIndex)sourceIndex;
                var di = (ElasticIndex)destinationIndex;

                di.Alias = si.Alias;
                di.AliasWithPrefix = si.AliasWithPrefix;
                si.Alias = string.Empty;
                si.AliasWithPrefix = string.Empty;
            }
            return response.Success;
        }

        public async Task<string> RunReindex(IReindexTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            JObject paramsObj = new JObject
            {
                {"conflicts", "proceed"},
                {"source", new JObject()
                    {
                        {"index", task.SourceIndex}
                    }
                },
                {"dest", new JObject()
                    {
                        {"index", task.DestinationIndex},
                        {"version_type", "external" }
                    }
                }
            };
            var body = paramsObj.ToString();
            var response = await Elastic.ReindexOnServerAsync<StringResponse>(body, new ReindexOnServerRequestParameters { WaitForCompletion = false });

            if (!response.Success)
            {
                throw new InvalidOperationException("Ошибка при попытке запустить переиндексацию", response.OriginalException);
            }
            var respObj = JObject.Parse(response.Body);
            var taskId = respObj.Value<string>("task");
            return taskId;
        }

        /// <summary>
        /// Локально создать экземпляр <see cref="IElasticIndex"/> (целевой индекс)
        /// на основе исходного <paramref name="sourceIndex"/> и даты создания <paramref name="creationDateTime"/>
        /// </summary>
        /// <returns>Новый экземпляр <see cref="IElasticIndex"/></returns>
        /// <remarks>
        /// Обращенияк Elastic НЕ ПРОИСХОДИТ. Экземпляр создается локально для приложения. Не более!
        /// </remarks>
        public IElasticIndex CreateDestinationIndexLocally(IElasticIndex sourceIndex, DateTime creationDateTime)
        {
            var newIndexInfo = IndexesParser.CreateDestinationIndexLocally(sourceIndex, creationDateTime);
            return new ElasticIndex(newIndexInfo);
        }

        public IElasticIndex CreateDestinationIndex(string indexFullName)
        {
            var newIndexInfo = IndexesParser.CreateIndexInfoFromName(indexFullName);
            var response = Elastic.Indices.Create<StringResponse>(indexFullName, null);

            if (!response.Success)
            {
                throw new InvalidOperationException("Ошибка при попытке создания индекса", response.OriginalException);
            }
            var respObj = JObject.Parse(response.Body);
            var acknowledged = respObj.Value<bool>("acknowledged");
            if (!acknowledged)
            {
                throw new InvalidOperationException("Ошибка при попытке создания индекса", response.OriginalException);
            }

            return new ElasticIndex(newIndexInfo);
        }
        private void CheckIndexCanBeManaged(IElasticIndex index)
        {
            if (index.Readonly)
            {
                throw new InvalidOperationException("Попытка изменения REadonly индекса");
            }
        }

    }
}
