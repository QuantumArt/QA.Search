using System;

namespace QA.Search.Generic.Integration.Core.Models
{
    /// <summary>
    /// Состояние процесса индексации одного индекса или группы индексов.
    /// </summary>
    public class IndexingReport
    {
        /// <summary>
        /// Логическое название индекса Elastic (без префиксов и суффиксов).
        /// Может быть не определено.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Размер партии документов, загружаемых из источника данных
        /// и отправляемых в Elastic за один вызов клиента.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Примерное количество документов, которое требуется загручить и проиндескировать.
        /// Может отличаться от <see cref="ProductsLoaded"/> в большую сторону, потому что
        /// документы могут быть отфильтрованы сервисом индексации в процессе загурзки.
        /// </summary>
        public int IdsLoaded { get; set; }

        /// <summary>
        /// Количество отправленных в Elastic документов 
        /// </summary>
        public int ProductsLoaded { get; set; }

        /// <summary>
        /// Количество проиндексированных документов 
        /// </summary>
        public int ProductsIndexed { get; set; }

        /// <summary>
        /// Время, потраченное на загрузку документов (с момента старта индексации)
        /// </summary>
        public TimeSpan DocumentsLoadTime { get; set; }

        /// <summary>
        /// Время, потраченное на препроцеессинг документов (с момента старта индексации)
        /// </summary>
        public TimeSpan DocumentsProcessTime { get; set; }

        /// <summary>
        /// Время, потраченное на индексацию документов (с момента старта индексации)
        /// </summary>
        public TimeSpan DocumentsIndexTime { get; set; }

        public IndexingReport()
        {
        }

        public IndexingReport(string indexName, int batchSize)
        {
            IndexName = indexName;
            BatchSize = batchSize;
        }

        public virtual void Clean()
        {
            IdsLoaded = 0;
            ProductsLoaded = 0;
            ProductsIndexed = 0;
            DocumentsLoadTime = TimeSpan.Zero;
            DocumentsProcessTime = TimeSpan.Zero;
            DocumentsIndexTime = TimeSpan.Zero;
        }
    }
}