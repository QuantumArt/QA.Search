using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement;
using QA.Search.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    /* ToDo: логика обработки каждой из групп индексов,
     * реализованная в данном классе, неплохо ложится на
     * паттерн "цепочка обязанностей"
     * Но окончательно понятно это стало несколько поздно (
     * Если доведется рефакторить - можно учесть этот факт
     */

    /// <summary>
    /// Компонент выполняет одну итерацию фоновой обработки задач
    /// </summary>
    public class ReindexWorkerIterationProcessor : IDisposable
    {
        #region injected

        private ILogger Logger { get; }

        private ElasticConnector ElasticConnector { get; }

        private ReindexTaskManager ReindexTaskManager { get; }

        private TimeSpan ProcessingInterval { get; }

        #endregion

        #region data

        private List<IElasticIndex> AllIndexes { get; set; }

        private List<IReindexTask> AllActiveTasks { get; set; }

        private List<IReindexTask> AllFinishedTasks { get; set; }

        #endregion

        private CancellationToken CancellationToken { get; set; }

        public ReindexWorkerIterationProcessor(ElasticConnector elasticConnector,
            ReindexTaskManager reindexTaskManager, ILogger logger, IOptions<ReindexWorkerSettings> options)
        {
            ElasticConnector = elasticConnector;
            ReindexTaskManager = reindexTaskManager;
            Logger = logger;
            ProcessingInterval = options.Value.Interval;

            Disposed = false;
        }

        #region public

        /// <summary>
        /// Выполняет одиночную итерацию для объекта <see cref="ReindexWorker"/>
        /// </summary>
        /// <returns>
        /// Новое состояние
        /// </returns>
        public async Task<IEnumerable<IIndexesContainer>> DoWork(CancellationToken cancellationToken)
        {
            CheckIsDisposed();
            CancellationToken = cancellationToken;
            var allData = new List<IIndexesContainer>();

            // 1. Получить все индексы
            await LoadIndexes();

            // 2. Получить из БД данные об активных тасках
            LoadTasks();

            // 3. Обработать пробламные задачи
            // await ProcessTaskWithProblems();

            // 4. Теперь загрузить последние завершенные задачи для индексов
            // Это будет актуально для построения модели, когда нет активных задач. -Пользователь увидит какая задача была завершена
            LoadFinishedTasks();

            // 5. Сгруппировать индексы по имени без учета даты в названии
            var indexesGroups = AllIndexes
                .GroupBy(i => i.UIName)
                .OrderBy(g => g.Key);

            // 5. Обработать группы
            foreach (var group in indexesGroups)
            {
                try
                {
                    IIndexesContainer containerForGroup = await ProcessGroup(group);
                    allData.Add(containerForGroup);
                }
                catch (ReindexWorkerIterationException reindexException)
                {
                    throw reindexException;
                }
                catch (Exception ex)
                {
                    var msg = $"При работе компонента {GetType().AssemblyQualifiedName} возникла неожидаемая ошибка";
                    Logger.LogError(ex, msg);
                    throw new ReindexWorkerIterationException(msg, ex);
                }
            }
            return allData;
        }

        #endregion

        private async Task ProcessTaskWithProblems()
        {
            await CancelTasksWhichHasNotRelatedIndex();

            // Если задача переведена в статусы первая/вторая индексация более пяти (с потолка) интервалов сработки
            // но, вместе с тем, идентификатор Elastic'овской таски отсутствует - то значит что реальная задача на эластике запущена не была

            var fiveIntervals = ProcessingInterval * 5;
            var now = DateTime.Now;

            var targetTasks = AllActiveTasks // Эти таски должны быть отмечены, как зафейленные
                .Where(t => t.Status == ReindexTaskStatus.ReindexOneAndAliasesSwap || t.Status == ReindexTaskStatus.ReindexTwo)
                .Where(t => string.IsNullOrWhiteSpace(t.ElasticTaskId))
                .Where(t => now - t.LastUpdated > fiveIntervals)
                .ToList();

            foreach (var failedTask in targetTasks)
            {
                AllActiveTasks.Remove(failedTask);
                await ElasticConnector.DeleteTaskIfPossible(failedTask);
                ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(failedTask, null, ReindexTaskStatus.Failed);
            }
        }

        private async Task CancelTasksWhichHasNotRelatedIndex()
        {
            var target = AllActiveTasks
                .Where(t => !AllIndexes.Any(i => i.FullName == t.SourceIndex));

            var ts = target
                .Select(CancelTask)
                .ToArray();

            await Task.WhenAll(ts);
        }

        private async Task LoadIndexes()
        {
            try
            {
                AllIndexes = (await ElasticConnector.GetAllIndexes(CancellationToken)).ToList();
            }
            catch (Exception ex)
            {
                var msg = "Ошибка при загрузке списка индексов";
                Logger.LogError(ex, msg);
                throw new ReindexWorkerIterationException(msg, ex);
            }
        }

        private void LoadTasks()
        {
            try
            {
                AllActiveTasks = ReindexTaskManager.GetAllActiveTasks().ToList();
            }
            catch (Exception ex)
            {
                var msg = "Ошибка при загрузке списка активных задач";
                Logger.LogError(ex, msg);
                throw new ReindexWorkerIterationException(msg, ex);
            }
        }

        private void LoadFinishedTasks()
        {
            try
            {
                AllFinishedTasks = ReindexTaskManager.GetFinishedTasks().ToList();
            }
            catch (Exception ex)
            {
                var msg = "Ошибка при загрузке списка завершенных задач";
                Logger.LogError(ex, msg);
                throw new ReindexWorkerIterationException(msg, ex);
            }
        }

        private async Task<IIndexesContainer> ProcessGroup(IGrouping<string, IElasticIndex> group)
        {
            var indexes = group
                .OrderBy(index => index.CreationDate)
                .ToList();

            if (indexes.Count > 2) // Если есть более двух индексов с одинаковыми именами, но разными датами - считаю ситуацию некорректной
            {
                return await ProcessGroupWhichContainsMoreThenTwoIndexes(indexes);
            }

            if (indexes.Count == 2) // Группа содержит два индекса
            {
                IElasticIndex firstIndex = indexes.First();
                IElasticIndex secondIndex = indexes.Skip(1).First();
                IReindexTask firstIndexTask = FindTaskForSourceIndex(firstIndex);
                // Проверка ненормальных ситуаций, когда есть два индекса (в ходе проверки количество индексов в группе может измениться)
                var checkRes = CheckAndFixGroupWhichContainsTwoIndexes(indexes, firstIndex, secondIndex, firstIndexTask);
                if (checkRes != null)
                {
                    return checkRes;
                }
                if (indexes.Count == 2) // Проверка не привела к изменению количества индексов в группе
                {
                    return await ProcessGroupWhichContainsTwoIndexes(indexes, firstIndex, secondIndex, firstIndexTask);
                }
            }
            if (indexes.Count == 1) // группа с одним индексом
            {
                return await ProcessGroupWhichContainsOneIndex(indexes);
            }
            throw new Exception("Ошибка реализации."); // Не должны мы тут оказаться
        }

        /// <summary>
        /// Проверить статус таски и обновить его (если надо)
        /// </summary>
        private async Task<IIndexesContainer> CheckAndUpdateReindexTaskStatus(IReindexTask reindexingTask, IElasticIndex sourceIndex = null,
            IElasticIndex destinationIndex = null)
        {
            switch (reindexingTask.Status) // Исходя из статуса, который задача имеет сейчас
            {
                case ReindexTaskStatus.AwaitStart:
                    //  Создать сущность нового индекса из имени.
                    destinationIndex = ElasticConnector.CreateDestinationIndex(reindexingTask.DestinationIndex);
                    // Таска ждет запуска
                    await RunReindexingForTask(reindexingTask);

                    return CreateNormalIndexesContainer(sourceIndex, destinationIndex, reindexingTask);
                case ReindexTaskStatus.ReindexOneAndAliasesSwap:
                case ReindexTaskStatus.ReindexTwo:
                    if (destinationIndex == null || reindexingTask.ElasticTaskId == null) // нет индекса назначения или Нет Id таски
                    {
                        await CancelTask(reindexingTask); // Непонятно, что делать с такой ситуацией
                        return CreateNormalIndexesContainer(sourceIndex);
                    }
                    await CheckStatusByTaskId(reindexingTask, sourceIndex, destinationIndex);
                    if (reindexingTask.Status == ReindexTaskStatus.Completed)
                    {
                        return CreateNormalIndexesContainer(destinationIndex, null, reindexingTask);
                    }
                    return CreateNormalIndexesContainer(sourceIndex, destinationIndex, reindexingTask);
            }
            return CreateNormalIndexesContainer(sourceIndex, destinationIndex, reindexingTask);
        }

        private async Task RunReindexingForTask(IReindexTask reindexingTask)
        {
            try
            {
                var taskId = await ElasticConnector.RunReindex(reindexingTask);

                CheckDbOpStatus(
                    ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, taskId, ReindexTaskStatus.ReindexOneAndAliasesSwap));
            }
            catch (Exception)
            {
                await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                CheckDbOpStatus(ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Failed));
                throw;
            }
        }

        private async Task CheckStatusByTaskId(IReindexTask reindexingTask, IElasticIndex sourceIndex,
            IElasticIndex destinationIndex = null)
        {
            if (reindexingTask == null)
            {
                throw new ArgumentNullException(nameof(reindexingTask));
            }
            if (string.IsNullOrEmpty(reindexingTask.ElasticTaskId))
            {
            }
            // Если есть идентификатор таски, то надо проверить
            var checkRes = await ElasticConnector.GetElasticTaskStatus(reindexingTask);
            var elStatus = checkRes.Status;

            switch (elStatus)
            {
                case TaskCheckingStatus.Error: // Ошибка взаимодействия
                    throw new ReindexWorkerIterationException("Ошибка взаимодействия с Elastic", null); // Может быть на следующей итерации оживет
                case TaskCheckingStatus.Failed:
                    await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                    ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Failed);
                    return;
                case TaskCheckingStatus.NotFound:
                    // Не вводи в заблуждение )
                    await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                    ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Failed);
                    return;
                case TaskCheckingStatus.InProgress:
                    // ToDo: Не меняется ничего - но для таски надо поставить прогресс
                    ReindexTaskManager.UpdateProgress(reindexingTask, checkRes);
                    break;
                case TaskCheckingStatus.Completed: // Задачка выполнена
                    switch (reindexingTask.Status) // Какой сейчас статус
                    {
                        case ReindexTaskStatus.ReindexOneAndAliasesSwap: // Завершилась первая переиндексация
                            try
                            {
                                var swapRes = await ElasticConnector.SwapAliases(sourceIndex, destinationIndex);
                                if (!swapRes)
                                {
                                    await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                                    ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Failed);
                                    return;
                                }
                                else
                                {
                                    //TODO find swap place
                                    // Алиасы перекинуты, в
                                    Logger.LogTrace("ReindexOneAndAliasesSwap swapped");
                                }

                                // Алиас перекинут - необходимо запустить повторную индексацию
                                var taskId = await ElasticConnector.RunReindex(reindexingTask);
                                await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                                // Пошла выполняться вторая переиндексация
                                ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, taskId, ReindexTaskStatus.ReindexTwo);
                                return;
                            }
                            catch (Exception e) // Ошибка смены алиасов
                            {
                                Logger.LogWarning(e, "ReindexingTask reindexOneAndAliasesSwap error");

                                // Задача будет считаться зафейленной
                                await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                                ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Failed);
                                return;
                            }
                        case ReindexTaskStatus.ReindexTwo: // Завершилась вторая переиндексация
                            try
                            {
                                // необходимо удалить исходный индекс
                                await ElasticConnector.RemoveIndex(reindexingTask.SourceIndex);
                                await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                                // Пометить таску в БД, как завершенную
                                ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Completed);
                                //sourceIndex = destinationIndex;
                                //destinationIndex = null;
                                return;
                            }
                            catch (Exception e)
                            {
                                Logger.LogWarning(e, "ReindexingTask reindexTwo error");

                                await ElasticConnector.DeleteTaskIfPossible(reindexingTask);
                                // Задача будет считаться зафейленной
                                ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(reindexingTask, null, ReindexTaskStatus.Failed);
                                return;
                            }
                    }
                    break;
            }
        }

        private void CheckDbOpStatus(ReindexTaskOperationStatus updateOperationStatus)
        {
            if (updateOperationStatus != ReindexTaskOperationStatus.Ok)
            {
                throw new ReindexWorkerIterationException("Не удалось обновить состояние задачи в БД", null);
            }
        }

        private async Task<IIndexesContainer> ProcessGroupWhichContainsOneIndex(List<IElasticIndex> indexes)
        {
            var singleIndex = indexes.First();
            var reindexingTask = FindTaskForSourceIndex(singleIndex);
            if (reindexingTask == null) // Самая обычная ситуация индекс есть и никаких тасок для него не запускалось
            {
                return CreateNormalIndexesContainer(singleIndex);
            }
            return await CheckAndUpdateReindexTaskStatus(reindexingTask, singleIndex);
        }

        /// <summary>
        /// Обработка группы, когда в ней два индекса
        /// </summary>
        /// <param name="indexes">Все индексы в группе (состав списка может быть изменен при работе метода)</param>
        /// <param name="firstIndex">первый индекс</param>
        /// <param name="secondIndex">второй индекс</param>
        /// <param name="reindexingTask">активная задача переиндексации, в которой исходным индексом является <paramref name="firstIndex"/></param>
        private async Task<IIndexesContainer> ProcessGroupWhichContainsTwoIndexes(List<IElasticIndex> indexes,
            IElasticIndex firstIndex, IElasticIndex secondIndex, IReindexTask reindexingTask)
        {
            bool taskIdNotFound = string.IsNullOrWhiteSpace(reindexingTask.ElasticTaskId);
            if (taskIdNotFound) // Нет сохраненного ID'шника таски
            {
                switch (reindexingTask.Status)
                {
                    case ReindexTaskStatus.AwaitStart: // AwaitStart, но уже есть второй индекс. Ошибка!!
                        return CreateWrongIndexesContainer(indexes);
                    case ReindexTaskStatus.ReindexOneAndAliasesSwap:
                    case ReindexTaskStatus.ReindexTwo:
                        // Ну ок - запустить переиндексацию
                        var taskId = await ElasticConnector.RunReindex(reindexingTask);
                        CheckDbOpStatus(
                            ReindexTaskManager.UpdateReindexElasticTaskId(reindexingTask, taskId)
                            );
                        return CreateNormalIndexesContainer(firstIndex, secondIndex, reindexingTask);
                    default:
                        await CancelTask(reindexingTask); // Непонятно, что за статус таски при наличии двух индексов
                        return CreateWrongIndexesContainer(indexes);
                }
            }
            // А вот тут и далее таска имеет идентификатор
            return await CheckAndUpdateReindexTaskStatus(reindexingTask, firstIndex, secondIndex);
        }

        /// <summary>
        /// Проверка корректности ситуации в случае, если в группе два индекса
        /// </summary>
        /// <param name="indexes">Все индексы в группе (состав списка может быть изменен при работе метода)</param>
        /// <param name="firstIndex">первый индекс</param>
        /// <param name="secondIndex">второй индекс</param>
        /// <param name="firstIndexTask">активная задача переиндексации, в которой исходным индексом является <paramref name="firstIndex"/></param>
        /// <returns>
        /// null - все нормально, обработку группы можно продолжать
        /// !null - контейнер нужно добавить в общее состояние. Обработку группы индексов не продолжать
        /// </returns>
        private IIndexesContainer CheckAndFixGroupWhichContainsTwoIndexes(List<IElasticIndex> indexes, IElasticIndex firstIndex,
            IElasticIndex secondIndex, IReindexTask firstIndexTask)
        {
            //TODO add await or remove async
            if ((firstIndex.HasAlias && secondIndex.HasAlias) ||// Алиасы сразу на двух индексах - некорректно с т.з. админки
                       (!firstIndex.HasAlias && !secondIndex.HasAlias)) // или ни на одном нет алиаса
            {
                return CreateWrongIndexesContainer(indexes); // Ошибочная ситуация - оповестить пользователя в UI
            }

            if (firstIndexTask == null) // Есть два индекса, но нет активной таски в БД
            {
                return CreateWrongIndexesContainer(indexes);
                //if (firstIndex.HasAlias) // Алиас на первом индексе
                //{
                //    // Непонятно тогда, откуда взялся второй индекс
                //    await RemoveIndex(secondIndex, indexes);
                //    secondIndex = null; // поскольку, размер списка indexes уменьшится на 1, то ни fistIndex? ни SecondIndex использоваться дальше уже не будет
                //    return null;
                //}
                //else if (secondIndex.HasAlias)
                //{
                //    // Алиас на втором, но что за ситуация - сказать нельзя, т.к. нет сведений о задаче - пусть пользователь разбирается с индексами сам
                //    return CreateWrongIndexesContainer(indexes); // Ошибочная ситуация - оповестить пользователя в UI
                //}
            }
            // Активная задача в БД есть

            //if (firstIndexTask.DestinationIndex != secondIndex.FullName) // Задача ко второму индексу отношения не имеет
            //{
            //    return CreateWrongIndexesContainer(indexes);
            //    if (!secondIndex.HasAlias) // Второй не имеет алиаса
            //    {
            //        await RemoveIndex(secondIndex, indexes); // Ну и не нужен он
            //        secondIndex = null; // поскольку, размер списка indexes уменьшится на 1, то ни fistIndex? ни SecondIndex использоваться дальше уже не будет
            //        return null;
            //    }
            //    else // У первого индекса нет алиаса, а у второго есть
            //    {
            //        // При наличии задачи это возможно, если выполняется (должно быть выполнено) второе индексирование
            //        if (firstIndexTask.Status != ReindexTaskStatus.ReindexTwo) // А если не выполняется
            //        {
            //            // Тогда ситуация оказывается непонятной и требует вмешательства пользователя
            //            await CancelTask(firstIndexTask);
            //            firstIndexTask = null;
            //            return CreateWrongIndexesContainer(indexes);
            //        }
            //        return null;
            //    }
            //}
            //// Есть таска и она предназначена для переиндексации из firstIndex в secondIndex
            //if (firstIndexTask.Status == ReindexTaskStatus.AwaitStart) // Таска только ожидает запуска, но второй индекс есть
            //{
            //    await RemoveIndex(secondIndex, indexes); // Убрать его - в indexes стало меньше (ToDo: неочевидный код)
            //    secondIndex = null;
            //    return null; // И дальше ситуация будет обработана, как если бы был только один индекс
            //}
            return null; // Вроде как не найдено условий некорректного состояния
        }

        /// <summary>
        /// Обработать заведомо ошибочную (содержащую > 2 индексов группу)
        /// </summary>
        private async Task<IIndexesContainer> ProcessGroupWhichContainsMoreThenTwoIndexes(List<IElasticIndex> indexes)
        {
            /* Если есть задача, реиндексации для любого из индексов, то в БД ставится отметка что задача
                принудительно завершена, и делается попытка завешить таску в Elasti'е*/
            var tasksForCancellation = indexes
                .Select(index => FindTaskForSourceIndex(index))
                .Where(task => task != null)
                .ToList();

            // Проставить в БД, что таски прерваны + завершить таски в Elastic
            var tasks = tasksForCancellation.Select(t => CancelTask(t));
            await Task.WhenAll(tasks.ToArray());
            return CreateWrongIndexesContainer(indexes);
        }

        private IIndexesContainer CreateWrongIndexesContainer(List<IElasticIndex> indexes)
        {
            return new IndexesContainer(indexes, lastFinishedReindexTask: GetLastFinishedTask(indexes.ToArray()));
        }

        private IReindexTask GetLastFinishedTask(params IElasticIndex[] indexes)
        {
            var targetIndexes = indexes.Where(i => i != null).ToList();
            var lastFinishedTask = AllFinishedTasks
                    .FirstOrDefault(t => targetIndexes.Any(i => t.SourceIndex == i.FullName || t.DestinationIndex == i.FullName));
            return lastFinishedTask;
        }

        private IIndexesContainer CreateNormalIndexesContainer(IElasticIndex sourceIndex,
            IElasticIndex destinationIndex = null, IReindexTask reindexTask = null)
        {
            IReindexTask lastFinishedTask = null;

            if (reindexTask == null) // Нет активной таски - ищу завершенную
            {
                //var sN = sourceIndex.FullName;
                //var dN = destinationIndex?.FullName;

                lastFinishedTask = GetLastFinishedTask(sourceIndex, destinationIndex);
                //AllFinishedTasks
                //.FirstOrDefault(t => t.SourceIndex == sN || t.DestinationIndex == sN
                //|| (destinationIndex != null && (t.SourceIndex == dN || t.DestinationIndex == dN)));
            }
            var container = new IndexesContainer(sourceIndex, destinationIndex, reindexTask, lastFinishedTask);
            return container;
        }

        private async Task RemoveIndex(IElasticIndex index, List<IElasticIndex> indexesGroup)
        {
            indexesGroup.Remove(index);
            await ElasticConnector.RemoveIndex(index);
        }

        private async Task CancelTask(IReindexTask task)
        {
            if (task != null)
            {
                AllActiveTasks.Remove(task);
                await ElasticConnector.CancelTaskIfPossible(task);
                await ElasticConnector.DeleteTaskIfPossible(task);
                ReindexTaskManager.UpdateReindexElasticTaskIdAndStatus(task, null, ReindexTaskStatus.CancelledByWorker);
            }
        }

        private IReindexTask FindTaskForSourceIndex(IElasticIndex sourceIndex)
        {
            return AllActiveTasks.FirstOrDefault(task => task.SourceIndex == sourceIndex.FullName);
        }

        #region Disposing

        private void CheckIsDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().AssemblyQualifiedName,
                    "Экземпляр класса должен использоваться для выполнения только одной операции одной");
            }
        }

        private bool Disposed { get; set; }

        public void Dispose()
        {
            Disposed = true;
        }

        #endregion
    }

    /// <summary>
    /// Класс - маркер для исключительной ситуации в процессе работы компонента
    /// </summary>
    public class ReindexWorkerIterationException : Exception
    {
        public ReindexWorkerIterationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}