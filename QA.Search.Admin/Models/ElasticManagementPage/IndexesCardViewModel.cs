using System;
using System.Collections.Generic;
using System.Linq;
using QA.Search.Data.Models;

namespace QA.Search.Admin.Models.ElasticManagementPage
{
    public class IndexesCardViewModel
    {
        /// <summary>
        /// Исходный (или единственный индекс)
        /// </summary>
        public ElasticIndexViewModel SourceIndex { get; set; }

        /// <summary>
        /// Целевой индекс для переиндексации
        /// </summary>
        /// <remarks>
        /// Заполняется только в том случае, если есть задача на переиндексацию.
        /// </remarks>
        public ElasticIndexViewModel DestinationIndex { get; set; }

        /// <summary>
        /// Все индексы, кроме самого раннего, если индексов больше двух
        /// </summary>
        /// <remarks>
        /// Наличие объектов в коллекции говорит о некорректном состоянии 
        /// индекса и невозможности запустить операцию индексации
        /// </remarks>
        public List<ElasticIndexViewModel> WrongIndexes { get; set; }

        /// <summary>
        /// Сведения об активной задаче переиндексации
        /// </summary>
        public ReindexTaskViewModel ReindexTask { get; set; }

        /// <summary>
        /// Сведения о последней завершенной задаче переиндексации
        /// </summary>
        public ReindexTaskViewModel LastFinishedReindexTask { get; set; }

        /// <summary>
        /// Может ли быть запущена новая задача переиндексации
        /// </summary>
        public bool CanRunNewTask
        {
            get
            {
                return !GetAnyIndexIsReadonly() &&
                    (!WrongIndexes.Any() && DestinationIndex == null && (ReindexTask == null || !TaskHasActiveStatus(ReindexTask.Status)) && SourceIndex != null);
            }
        }

        public bool HasTaskWithActiveStatus
        {
            get
            {
                return ReindexTask != null && TaskHasActiveStatus(ReindexTask.Status);
            }
        }

        private bool TaskHasActiveStatus(ReindexTaskStatus status)
        {
            switch (status)
            {
                case ReindexTaskStatus.AwaitStart:
                case ReindexTaskStatus.ReindexOneAndAliasesSwap:
                case ReindexTaskStatus.ReindexTwo:
                    return true;
                default: return false;
            }
        }

        public bool IsReadonly => GetAnyIndexIsReadonly();


        private bool GetAnyIndexIsReadonly()
        {
            var anyIndexInContainerIsReadonly = (SourceIndex?.Readonly ?? false)
                    || (DestinationIndex?.Readonly ?? false)
                    || (WrongIndexes?.Any(i => i.Readonly) ?? false);
            return anyIndexInContainerIsReadonly;
        }
    }

}
