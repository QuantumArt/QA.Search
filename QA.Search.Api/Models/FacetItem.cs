using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Фасет по одному полю
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FacetItem
    {
        /// <summary>
        /// Границы значения поля в выборке
        /// </summary>
        public IntervalFacet Interval { get; set; }

        /// <summary>
        /// Часто встречающиеся значения поля в выборке
        /// </summary>
        public SampleFacet[] Samples { get; set; }

        /// <summary>
        /// Статистическое распределение значений поля
        /// </summary>
        public PercentileFacet[] Percentiles { get; set; }

        /// <summary>
        /// Распределение количества документов по каждому указанному интервалу
        /// </summary>
        public RangeFacet[] Ranges { get; set; }
    }

    /// <summary>
    /// Границы значения поля в выборке
    /// </summary>
    public class IntervalFacet
    {
        /// <summary>
        /// Минимальное значение
        /// </summary>
        [Required]
        public JValue From { get; set; }

        /// <summary>
        /// Максимальное значение
        /// </summary>
        [Required]
        public JValue To { get; set; }
    }

    /// <summary>
    /// Часто встречающееся значение поля в выборке
    /// </summary>
    public class SampleFacet
    {
        /// <summary>
        /// Значение поля
        /// </summary>
        [Required]
        public JValue Value { get; set; }

        /// <summary>
        /// Количество документов
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Значение указанной квантили
    /// </summary>
    public class PercentileFacet
    {
        /// <summary>
        /// Квантиль в процентах
        /// </summary>
        public float Percent { get; set; }

        /// <summary>
        /// Значение поля
        /// </summary>
        [Required]
        public JValue Value { get; set; }
    }

    /// <summary>
    /// Количество документов в указаной группе
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class RangeFacet
    {
        /// <summary>
        /// Название группы документов
        /// </summary>
        [Required]
        public JValue Name { get; set; }

        /// <summary>
        /// Минимальное значение
        /// </summary>
        public JValue From { get; set; }

        /// <summary>
        /// Максимальное значение
        /// </summary>
        public JValue To { get; set; }

        /// <summary>
        /// Количество документов
        /// </summary>
        public int Count { get; set; }
    }
}
