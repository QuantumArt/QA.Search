using QA.Search.Generic.Integration.Core.Services;
using System;

namespace QA.Search.Generic.Integration.Core.Models
{
    /// <summary>
    /// Состояние одного процесса индексации определяемого ScheduledService
    /// </summary>
    public class IndexingContext<TMarker>
        where TMarker : IServiceMarker
    {
        public IndexingContext()
        {
            Iteration = 0;
        }

        /// <summary>
        /// Состояние сервиса индексации
        /// </summary>
        public IndexingState State { get; set; }

        /// <summary>
        /// Прогресс процесса индексации в процентах
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Текущий статус процесса индексации
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Кол-во запусков процесса индексации со старта приложения
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// Дата и время запуска индексации
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Дата и время завершения / остановки индексации
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Даты нескольких следующих запланированных запусков индексации
        /// </summary>
        public DateTime[] ScheduledDates { get; set; }

        /// <summary>
        /// Вычислить суффикс индекса Elastic на основе даты и времени
        /// начала индексации в формате yyyy-MM-ddTHH-mm-ss
        /// </summary>
        public string GetDateSuffix()
        {
            return StartDate
                .GetValueOrDefault()
                .ToUniversalTime()
                .ToString("s")
                .Replace(":", "-")
                .ToLower();
        }
    }
}