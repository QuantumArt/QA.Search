using System.Collections.Generic;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    /// <summary>
    /// Объект состояния хранящий сведения по индексу (группе индексов)
    /// и связанной с ними задаче
    /// </summary>
    public interface IIndexesContainer
    {
        /// <summary>
        /// Исходный (или единственный индекс)
        /// </summary>
        IElasticIndex SourceIndex { get; }

        /// <summary>
        /// Целевой индекс для переиндексации
        /// </summary>
        /// <remarks>
        /// Заполняется только в том случае, если есть задача на переиндексацию.
        /// </remarks>
        IElasticIndex DestinationIndex { get; }

        /// <summary>
        /// Все индексы, кроме самого раннего, если индексов больше двух
        /// </summary>
        /// <remarks>
        /// Наличие объектов в коллекции говорит о некорректном состоянии
        /// индекса и невозможности запустить операцию индексации
        /// </remarks>
        List<IElasticIndex> WrongIndexes { get; }

        /// <summary>
        /// Сведения об активной задаче переиндексации
        /// </summary>
        IReindexTask ReindexTask { get; }

        /// <summary>
        /// Сведения о последней завершенной задаче переиндексации
        /// </summary>
        IReindexTask LastFinishedReindexTask { get; }

        /// <summary>
        /// Исходный индекс в данном контейнере
        /// есть и совпадает с переданным по имени
        /// </summary>
        bool HasSourceIndex(string sourceIndexFullName);

        /// <summary>
        /// Ошибочные индексы содержат переданный индекс
        /// (совпадение по имени)
        /// </summary>
        bool WrongIndexesContains(string indexFullName);
    }
}