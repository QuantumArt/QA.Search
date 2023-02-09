using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Data;
using QA.Search.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ReindexTaskEntity = QA.Search.Data.Models.ReindexTask;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement
{

    /// <summary>
    /// Компонент, работающий с задачами.
    /// </summary>
    /// <remarks>
    /// Не позволяет выполнить одновременно, из разных потоков, несколько 
    /// операций по созданию или обновлению задач.
    /// </remarks>
    public class ReindexTaskManager
    {
        private class ReindexTask : IReindexTask
        {
            public ReindexTask(ReindexTaskEntity entity)
            {
                Status = entity.Status;
                SourceIndex = entity.SourceIndex;
                DestinationIndex = entity.DestinationIndex;
                ElasticTaskId = entity.ElasticTaskId;
                Created = entity.Created;
                Finished = entity.Finished;
                LastUpdated = entity.LastUpdated;
            }

            /// <summary>
            /// Текущий статус задачи
            /// </summary>
            public ReindexTaskStatus Status { get; set; }

            /// <summary>
            /// Исходный индекс для переиндексации
            /// </summary>
            public string SourceIndex { get; set; }

            /// <summary>
            /// В какой индек проводится переиндексация
            /// </summary>
            public string DestinationIndex { get; set; }

            /// <summary>
            /// Идентификатор задачи в Elastic'e
            /// </summary>
            public string ElasticTaskId { get; set; }

            /// <summary>
            /// Дата и вемя создания задачи
            /// </summary>
            public DateTimeOffset Created { get; set; }

            /// <summary>
            /// Дата и вемя завершения задачи
            /// </summary>
            public DateTimeOffset? Finished { get; set; }

            public DateTimeOffset LastUpdated { get; set; }

            private CheckTaskStatusResult CheckTaskStatusResult { get; set; }

            public void UpdateProgress(CheckTaskStatusResult data)
            {
                switch (data.Status)
                {
                    case TaskCheckingStatus.Error:
                    case TaskCheckingStatus.Failed:
                    case TaskCheckingStatus.NotFound:
                        CheckTaskStatusResult = null;
                        break;
                    default:
                        CheckTaskStatusResult = data;
                        break;
                }
            }

            public int TotalDocuments => CheckTaskStatusResult?.TotalDocuments ?? 0;

            public int CreatedDocuments => CheckTaskStatusResult?.CreatedDocuments ?? 0;
            public int UpdatedDocuments => CheckTaskStatusResult?.UpdatedDocuments ?? 0;
            public int DeletedDocuments => CheckTaskStatusResult?.DeletedDocuments ?? 0;

            public int Percentage => CheckTaskStatusResult?.Percentage ?? 0;

            public TimeSpan TotalTime => CheckTaskStatusResult?.TotalTime ?? TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Задачи, имеющие данный статус не будут подгружаться из БД и останутся только для истории
        /// </summary>
        private ReindexTaskStatus[] UnactiveTaskStatuses { get; }

        #region Injected

        private ILogger Logger { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion



        public ReindexTaskManager(ILogger logger, IServiceScopeFactory serviceScopeFactory)
        {
            UnactiveTaskStatuses = new ReindexTaskStatus[] {
                ReindexTaskStatus.CancelledByWorker,
                ReindexTaskStatus.Completed,
                ReindexTaskStatus.Failed };
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
        }

        private TRes ExecuteWithContext<TRes>(Func<AdminSearchDbContext, TRes> func)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AdminSearchDbContext>();
                return func(dbContext);
            }
        }

        private void ExecuteWithContext(Action<AdminSearchDbContext> action)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AdminSearchDbContext>();
                action(dbContext);
            }
        }

        #region CreateReindexTask

        public ReindexTaskOperationStatus CreateReindexTask(IElasticIndex sourceIndex,
            IElasticIndex destinationIndex, DateTime creationDate, out IReindexTask newTask)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AdminSearchDbContext>();
                return CreateReindexTaskInternal(dbContext, sourceIndex, destinationIndex, creationDate, out newTask);
            }
        }

        private ReindexTaskOperationStatus CreateReindexTaskInternal(AdminSearchDbContext dbContext, IElasticIndex sourceIndex,
            IElasticIndex destinationIndex, DateTime creationDate, out IReindexTask newTask)
        {
            var thereIsExitedActiveTaskForSourceIndex = dbContext
                .ReindexTasks
                .Where(t => !UnactiveTaskStatuses.Contains(t.Status))
                .Where(t => t.SourceIndex == sourceIndex.FullName)
                .Any();

            if (thereIsExitedActiveTaskForSourceIndex)
            {
                newTask = null;
                return ReindexTaskOperationStatus.ThereIsActiveTaskForSourceIndex;
            }

            var newTaskEntity = new ReindexTaskEntity()
            {
                SourceIndex = sourceIndex.FullName,
                DestinationIndex = destinationIndex.FullName,
                ShortIndexName = sourceIndex.UIName,
                ElasticTaskId = null,
                Status = ReindexTaskStatus.AwaitStart,
                Created = creationDate,
                LastUpdated = creationDate,
                Finished = null
            };

            dbContext.ReindexTasks.Add(newTaskEntity);

            try
            {
                dbContext.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Logger.LogWarning(e, "CreateReindexTaskInternal db error");
                // ToDo: отдельная логика для определения задвоения записи
                newTask = null;
                return ReindexTaskOperationStatus.ThereIsActiveTaskForSourceIndex;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "CreateReindexTaskInternal general error");
                newTask = null;
                return ReindexTaskOperationStatus.DbError;
            }
            newTask = new ReindexTask(newTaskEntity);
            return ReindexTaskOperationStatus.Ok;
        }

        #endregion

        /// <summary>
        /// Получить все активные в данный момент таски
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IReindexTask> GetAllActiveTasks()
        {
            return ExecuteWithContext((dbContext) =>
            {
                return dbContext.ReindexTasks
                    .Where(t => !UnactiveTaskStatuses.Contains(t.Status))
                    .ToList()
                    .Select(e => new ReindexTask(e));
            });
        }

        /// <summary>
        /// Получить последнюю завершенную задачу доля каждого из индексов
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IReindexTask> GetFinishedTasks()
        {
            return ExecuteWithContext((dbContext) =>
            {
                var getNotActiveTasksQuery = dbContext.ReindexTasks
                .Join(
                    dbContext.ReindexTasks
                        .Where(rt => UnactiveTaskStatuses.Contains(rt.Status))
                        .GroupBy(rt => rt.ShortIndexName)
                        .Select(rt => new
                        {
                            ShortIndexName = rt.Key,
                            Timestamp = rt.Max(grt => grt.Timestamp)
                        }),
                ou => new { ou.ShortIndexName, ou.Timestamp },
                inn => new { inn.ShortIndexName, inn.Timestamp },
                (ou, inn) => ou);


                return getNotActiveTasksQuery.ToList()
                    .Select(e => new ReindexTask(e))
                    .ToList();
            });
        }

        private ReindexTaskOperationStatus UpdateReindexTask(IReindexTask task,
            Action<ReindexTaskEntity, ReindexTask> update,
            Action<ReindexTask> rollback)
        {
            return ExecuteWithContext((dbContext) =>
            {
                var dbTaskEntity = dbContext.ReindexTasks.Find(task.SourceIndex, task.DestinationIndex); // Таска в БД

                if (dbTaskEntity == null) // Нужная таска в БД не найдена
                {
                    return ReindexTaskOperationStatus.TaskNotFound;
                }

                try
                {
                    var tsk = (ReindexTask)task;
                    update(dbTaskEntity, tsk);
                    var now = DateTime.UtcNow;
                    tsk.LastUpdated = now;
                    dbTaskEntity.LastUpdated = now;

                    dbContext.SaveChanges();
                    return ReindexTaskOperationStatus.Ok;
                }
                catch (Exception)
                {
                    rollback?.Invoke((ReindexTask)task);
                    return ReindexTaskOperationStatus.DbError;
                }
            }

            //    UpdateReindexTaskInternal(dbContext, task, update, rollback)
            );
        }

        public ReindexTaskOperationStatus UpdateReindexElasticTaskId(IReindexTask task, string elasticTaskId)
        {
            var previousElasticTaskId = task.ElasticTaskId;
            return UpdateReindexTask(
                task,
                (dbEntity, taskObject) =>
                {
                    dbEntity.ElasticTaskId = elasticTaskId;
                    taskObject.ElasticTaskId = elasticTaskId;
                },
                taskObject => { taskObject.ElasticTaskId = previousElasticTaskId; });
        }

        public ReindexTaskOperationStatus UpdateReindexElasticTaskIdAndStatus(IReindexTask task, string elasticTaskId, ReindexTaskStatus newStatus)
        {
            var previousElasticTaskId = task.ElasticTaskId;
            return UpdateReindexTask(
                task,
                (dbEntity, taskObject) =>
                {
                    dbEntity.Status = newStatus;
                    dbEntity.ElasticTaskId = elasticTaskId;
                    taskObject.ElasticTaskId = elasticTaskId;
                    taskObject.Status = newStatus;
                    if (newStatus == ReindexTaskStatus.Completed)
                    {
                        var now = DateTime.UtcNow;
                        dbEntity.Finished = now;
                        taskObject.Finished = now;
                    }
                },
                taskObject => { taskObject.ElasticTaskId = previousElasticTaskId; });
        }

        internal void UpdateProgress(IReindexTask reindexingTask, CheckTaskStatusResult checkRes)
        {
            if ((ReindexTask)reindexingTask != null)
            {
                ((ReindexTask)reindexingTask).UpdateProgress(checkRes);
            }
        }
    }
}

