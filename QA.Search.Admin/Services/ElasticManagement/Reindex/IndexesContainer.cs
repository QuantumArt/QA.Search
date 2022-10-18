using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    /// <summary>
    /// Данные об индексах одной группы (одинаковые имена, но разные даты)
    /// И задаче связанной с ними
    /// </summary>
    public class IndexesContainer : IIndexesContainer
    {
        public IndexesContainer(IEnumerable<IElasticIndex> wrongIndexes, IReindexTask lastFinishedReindexTask)
        {
            WrongIndexes = wrongIndexes?.ToList() ?? new List<IElasticIndex>();
            LastFinishedReindexTask = lastFinishedReindexTask;
        }

        public IndexesContainer(IElasticIndex sourceIndex,
            IElasticIndex destinationIndex = null, IReindexTask reindexTask = null,
            IReindexTask lastFinishedReindexTask = null)
        {
            SourceIndex = sourceIndex;
            DestinationIndex = destinationIndex;
            ReindexTask = reindexTask;
            LastFinishedReindexTask = lastFinishedReindexTask;
            WrongIndexes = new List<IElasticIndex>();
        }

        public IElasticIndex SourceIndex { get; set; }

        public IElasticIndex DestinationIndex { get; set; }

        public List<IElasticIndex> WrongIndexes { get; }

        public IReindexTask ReindexTask { get; set; }

        /// <summary>
        /// Сведения о последней завершенной задаче переиндексации
        /// </summary>
        public IReindexTask LastFinishedReindexTask { get; set; }

        /// <summary>
        /// Исходный индекс в данном контейнере
        /// есть и совпадает с переданным по имени
        /// </summary>
        public bool HasSourceIndex(string sourceIndexFullName)
        {
            if (string.IsNullOrWhiteSpace(sourceIndexFullName))
            {
                throw new ArgumentException(nameof(sourceIndexFullName));
            }
            return SourceIndex != null
                && SourceIndex.FullName == sourceIndexFullName;
        }

        /// <summary>
        /// Ошибочные индексы содержат переданный индекс
        /// (совпадение по имени)
        /// </summary>
        public bool WrongIndexesContains(string indexFullName)
        {
            if (string.IsNullOrWhiteSpace(indexFullName))
            {
                throw new ArgumentException(nameof(indexFullName));
            }

            return WrongIndexes?.Any(wi => wi.FullName == indexFullName) ?? false;
        }
    }
}