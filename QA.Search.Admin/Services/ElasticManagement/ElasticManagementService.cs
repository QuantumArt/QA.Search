using Microsoft.Extensions.Logging;
using QA.Search.Admin.Models;
using QA.Search.Admin.Models.ElasticManagementPage;
using QA.Search.Admin.Services.ElasticManagement.Reindex;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement
{
    public class ElasticManagementService : ServiceBase
    {
        ReindexWorker ReindexWorker { get; set; }

        //public ElasticManagementService(ILogger logger, IHostedServiceAccessor<ReindexWorker> reindexWorkerAccessor) : base(logger)
        public ElasticManagementService(ILogger logger, ReindexWorker reindexWorker) : base(logger)
        {
            ReindexWorker = reindexWorker;
        }

        protected override string ErrorMessage { get; } = "Ошибка в работе сервиса управления Elastic";

        #region LoadData

        public ElasticManagementPageResponse LoadData()
        {
            return WrapAction(LoadDataInternal);
        }

        public ElasticManagementPageResponse LoadDataInternal()
        {
            var state = ReindexWorker.GetState();
            var allContainers = state.GetAllContainers();

            return new ElasticManagementPageResponse
            {
                Loading = !state.Loaded,
                CommonError = state.CommonError,
                Cards = state.Loaded && !state.CommonError
                    ? allContainers.Select(CreateCardViewModel).ToList()
                    : new List<IndexesCardViewModel>()
            };
        }

        private IndexesCardViewModel CreateCardViewModel(IIndexesContainer container)
        {
            var model = new IndexesCardViewModel
            {
                SourceIndex = CreateIndexViewModel(container.SourceIndex),
                DestinationIndex = CreateIndexViewModel(container.DestinationIndex),
                WrongIndexes = container.WrongIndexes?.Select(CreateIndexViewModel).ToList(),
                ReindexTask = CreateReindexTaskViewModel(container.ReindexTask),
                LastFinishedReindexTask = CreateReindexTaskViewModel(container.LastFinishedReindexTask)
            };
            return model;
        }

        internal Task RemoveIndex(string indexFullName)
        {
            return ReindexWorker.RemoveIndex(indexFullName);
        }

        internal async Task CreateNewIndex(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentException(nameof(indexName));
            }
            await ReindexWorker.CreateNewIndex(indexName);
        }

        private ReindexTaskViewModel CreateReindexTaskViewModel(IReindexTask reindexTask)
        {
            if (reindexTask == null)
            {
                return null;
            }

            return new ReindexTaskViewModel
            {
                SourceIndex = reindexTask.SourceIndex,
                DestinationIndex = reindexTask.DestinationIndex,
                Created = reindexTask.Created,
                LastUpdated = reindexTask.LastUpdated,
                Finished = reindexTask.Finished,
                Status = reindexTask.Status,
                TotalTime = reindexTask.TotalTime,
                TotalDocuments = reindexTask.TotalDocuments,
                CreatedDocuments = reindexTask.CreatedDocuments,
                UpdatedDocuments = reindexTask.UpdatedDocuments,
                DeletedDocuments = reindexTask.DeletedDocuments,
                Percentage = reindexTask.Percentage
            };
        }

        private ElasticIndexViewModel CreateIndexViewModel(IElasticIndex index)
        {
            if (index == null)
            {
                return null;
            }
            return new ElasticIndexViewModel
            {
                Alias = index.Alias,
                CreationDate = index.CreationDate,
                FullName = index.FullName,
                UIName = index.UIName,
                HasAlias = index.HasAlias,
                Readonly = index.Readonly
            };


        }

        #endregion


        #region CreateReindexTask

        public CreateReindexTaskResponse CreateReindexTask(string sourceIndex)
        {
            return WrapAction(() => CreateReindexTaskInternal(sourceIndex));
        }

        private CreateReindexTaskResponse CreateReindexTaskInternal(string sourceIndex)
        {
            var wRes = ReindexWorker.CreateReindexTask(sourceIndex);
            if (wRes == ReindexTaskOperationStatus.Ok)
            {
                return new CreateReindexTaskResponse { TaskCreated = true };
            }
            Logger.LogWarning("Не удалось создать задачу для переиндексации индекса {0}. Код результата {1}", sourceIndex, wRes);
            string message = "Ошибка при попытке создания задачи на переиндексацию";
            switch (wRes)
            {
                case ReindexTaskOperationStatus.DataIsNotLoaded:
                    message = "Данные не были загружены. Повторите попытку позднее.";
                    break;
                case ReindexTaskOperationStatus.DbError:
                    message = "Ошибка записи в БД  Повторите попытку позднее.";
                    break;
                case ReindexTaskOperationStatus.IncorrectIndexesState:
                    message = "Некорректное состояние индексов. Необходимо исправить вручную.";
                    break;
                case ReindexTaskOperationStatus.ServiceCanNotExecuteTask:
                    message = "Невозможно создать новую задачу по причине общего сбоя";
                    break;
                case ReindexTaskOperationStatus.SourceIndexNotFound:
                    message = "Исходный индекс не найден";
                    break;
                case ReindexTaskOperationStatus.ThereIsActiveTaskForSourceIndex:
                    message = "Для указанного исходного индекса есть активная задача переиндексации";
                    break;
            }
            return new CreateReindexTaskResponse
            {
                ErrorMessage = message,
                TaskCreated = false
            };
        }

        #endregion
    }
}

