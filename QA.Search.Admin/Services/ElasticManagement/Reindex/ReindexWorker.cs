using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public partial class ReindexWorker
    {
        #region injected

        private Func<ReindexWorkerIterationProcessor> IterationProcessorFactory { get; }

        private ReindexTaskManager ReindexTaskManager { get; }

        private ILogger Logger { get; }

        private ReindexWorkerSettings Settings { get; }

        private ElasticConnector ElasticConnector { get; }

        #endregion

        private ReindexState CurrentState { get; }

        public ReindexWorker(Func<ReindexWorkerIterationProcessor> iterationProcessorFactory,
            ReindexTaskManager reindexTaskManager, ILogger logger, IOptions<ReindexWorkerSettings> options,
            ElasticConnector elasticConnector)
        {
            IterationProcessorFactory = iterationProcessorFactory;
            ReindexTaskManager = reindexTaskManager;
            Logger = logger;
            ElasticConnector = elasticConnector;
            CurrentState = new ReindexState();

            Settings = options.Value;
        }

        public ReindexTaskOperationStatus CreateReindexTask(string sourceFullIndexName)
        {
            if (string.IsNullOrWhiteSpace(sourceFullIndexName))
            {
                throw new ArgumentException(nameof(sourceFullIndexName));
            }

            if (!CurrentState.Loaded)
            {
                return ReindexTaskOperationStatus.DataIsNotLoaded;
            }

            if (CurrentState.CommonError)
            {
                return ReindexTaskOperationStatus.ServiceCanNotExecuteTask;
            }

            var targetStateContainer = CurrentState
                .GetAllContainers()
                .Where(c => c.HasSourceIndex(sourceFullIndexName) || c.WrongIndexesContains(sourceFullIndexName))
                .FirstOrDefault();

            if (targetStateContainer == null)
            {
                return ReindexTaskOperationStatus.SourceIndexNotFound;
            }
            if (targetStateContainer.WrongIndexes.Any())
            {
                return ReindexTaskOperationStatus.IncorrectIndexesState;
            }
            if (targetStateContainer.ReindexTask != null || targetStateContainer.DestinationIndex != null)
            {
                return ReindexTaskOperationStatus.ThereIsActiveTaskForSourceIndex;
            }


            var sourceIndex = targetStateContainer.SourceIndex;

            var creationDate = DateTime.UtcNow;
            IElasticIndex destinationIndex = ElasticConnector.CreateDestinationIndexLocally(sourceIndex, creationDate);
            var res = ReindexTaskManager.CreateReindexTask(sourceIndex, destinationIndex, creationDate, out IReindexTask newTask);
            if (res != ReindexTaskOperationStatus.Ok)
            {
                return res;
            }
            // Созданная задача должна попасть в State, чтобы была возможность отобразить ее
            // Иными словами: запихнуть вновь созданную задачу в существующий контейнер
            CurrentState.UpdateContainer(
                targetStateContainer,
                sourceIndex,
                destinationIndex,
                null,
                newTask);

            return ReindexTaskOperationStatus.Ok;
        }

        internal async Task RemoveIndex(string indexFullName)
        {
            await ElasticConnector.RemoveIndex(indexFullName);
            CurrentState.RemoveIndexFromContainer(indexFullName);
        }

        internal async Task CreateNewIndex(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentException(nameof(indexName));
            }

            if (!CurrentState.Loaded || CurrentState.CommonError)
            {
                throw new InvalidOperationException();

            }

            IElasticIndex newIndex = await ElasticConnector.CreateNewIndex(indexName);
            CurrentState.CreateNewContainerForIndex(newIndex);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            if (!Settings.RunTasks)
            {
                return;
            }
            try
            {
                using (var iterationProcessor = IterationProcessorFactory())
                {
                    var newStateData = await iterationProcessor.DoWork(cancellationToken);
                    CurrentState.UpdateState(newStateData);
                }
            }
            catch (OperationCanceledException canceledEx)
            {
                throw canceledEx;
            }
            catch (ReindexWorkerIterationException ex)
            {
                Logger.LogError(ex, "При работе компонента {0} возникла ошибка", GetType().AssemblyQualifiedName);
                CurrentState.UpdateState(null);
            }
            catch (Exception ex) // Ошибка, которая не была обработана нигде выше
            {
                Logger.LogCritical(ex, "При работе компонента {0} возникла неизвестная ошибка", GetType().AssemblyQualifiedName);
                CurrentState.UpdateState(null);
            }
        }
    }
}
