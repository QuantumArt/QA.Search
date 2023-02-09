using System;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Data.Models
{
    /// <summary>
    /// Статус процесса бесшовной переиндексации одного индекса Elastic
    /// </summary>
    public enum ReindexTaskStatus
    {
        AwaitStart = 0,
        ReindexOneAndAliasesSwap = 1,
        ReindexTwo = 2,
        Completed = 3,
        Failed = 4,
        /// <summary>
        /// Задача была остановлена <see cref="ReindexWorker"/>, т.к. для одних и тех же индексов 
        /// было запущено более одной задачи
        /// </summary>
        CancelledByWorker = 5,
    }

    /// <summary>
    /// Модель процесса бесшовной переиндексации одного индекса Elastic
    /// </summary>
    public class ReindexTask
    {
        /// <summary>
        /// Текущий статус задачи
        /// </summary>
        public ReindexTaskStatus Status { get; set; }

        /// <summary>
        /// Исходный индекс для переиндексации
        /// </summary>
        public string SourceIndex { get; set; }

        /// <summary>
        /// В какой индекс проводится переиндексация
        /// </summary>
        public string DestinationIndex { get; set; }

        /// <summary>
        /// Логическое имя индекса, объединяющее Source и Destination
        /// </summary>
        public string ShortIndexName { get; set; }

        /// <summary>
        /// Идентификатор задачи в Elastic'e
        /// </summary>
        public string ElasticTaskId { get; set; }

        /// <summary>
        /// Дата и вемя создания задачи
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Дата и время последнего обновления
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }

        /// <summary>
        /// Дата и вемя завершения задачи
        /// </summary>
        public DateTimeOffset? Finished { get; set; }

        /// <summary>
        /// Row version
        /// </summary>
        public uint Timestamp { get; set; }
    }
}