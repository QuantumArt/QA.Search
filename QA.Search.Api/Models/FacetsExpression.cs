using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Набор агрегаций по различным полям для фасетного поиска
    /// </summary>
    /// <example>
    /// {
    ///   "Price": "$interval"
    ///   "Tags.Title": { "$samples": 10 },
    ///   "Parameters": {
    ///     "Speed": {
    ///       "$ranges": [
    ///         { "$key": "slow", "$to": 10 },
    ///         { "$key": "medium", "$from": 10, "$to": 50 },
    ///         { "$key": "fast", "$from": 50 }
    ///       ]
    ///     }
    ///   }
    /// }
    /// </example>
    [JsonObject]
    public class FacetsExpression : FacetExpression, IEnumerable<KeyValuePair<string, FacetExpression>>
    {
        /// <summary>
        /// Набор фасетов по разным полям документа
        /// </summary>
        [JsonIgnore]
        private List<KeyValuePair<string, FacetExpression>> _fields
            = new List<KeyValuePair<string, FacetExpression>>();

        public IEnumerator<KeyValuePair<string, FacetExpression>> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _props;

        /// <summary>
        /// Преобразует все вложенные объекты в набор плоских путей
        /// https://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
        /// </summary>
        /// <example>
        /// {
        ///   "Tags": { "Title": "$samples" },
        ///   "Parameters": { "Price": { "$percentiles": [0, 5, 50, 95, 100] } }
        /// }
        /// ▼▼▼
        /// {
        ///   "Tags.Title": { "$samples": 100 },
        ///   "Parameters.Price": { "$percentiles": [0, 5, 50, 95, 100] }
        /// }
        /// </example>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _fields = new List<KeyValuePair<string, FacetExpression>>();

            if (_props != null)
            {
                foreach (var (name, token) in _props)
                {
                    FacetsExpression expression = ToExpresion(token);

                    if (expression._fields.Count > 0)
                    {
                        foreach (var (innerName, innerFacet) in expression._fields)
                        {
                            string path = name + "." + innerName;

                            _fields.Add(path, innerFacet);
                        }
                    }
                    else
                    {
                        _fields.Add(name, expression);
                    }
                }

                _props = null;
            }
        }

        /// <summary>
        /// Преобразует сокращенный строковый синтаксис в объектные выражения
        /// </summary>
        private static FacetsExpression ToExpresion(JToken token)
        {
            if (token is JValue value)
            {
                switch ((string)value.Value)
                {
                    case "$interval":
                        // FacetExpression
                        return new FacetsExpression { Interval = true };

                    case "$samples":
                        // FacetExpression
                        return new FacetsExpression { Samples = 100 };
                }
            }
            // FacetsExpression or FacetExpression
            return token.ToObject<FacetsExpression>();
        }
    }

    /// <summary>
    /// Фасет по одному полю документа
    /// </summary>
    public class FacetExpression
    {
        /// <summary>
        /// Подсчет минимального и максимального значения поля в выборке
        /// </summary>
        /// <example>
        /// "$interval"
        /// </example>
        [JsonIgnore]
        public bool Interval { get; set; }

        /// <summary>
        /// Нахождение указанного числа наиболее популярных значений поля в выборке
        /// с подсчетом количества документов, соответствущих этому значению
        /// </summary>
        /// <example>
        /// "$samples"
        /// { "$samples": 10 }
        /// </example>
        [JsonProperty("$samples")]
        public int? Samples { get; set; }

        /// <summary>
        /// Построение медианного значения или доверительных интервалов по заданному полю
        /// </summary>
        /// <example>
        /// [50] // медиана
        /// [5, 95] // доверительный интервал 5-95 %
        /// [90, 99, 99.9, 99.99] // уровни доверия в процентах
        /// </example>
        [JsonProperty("$percentiles")]
        public int[] Percentiles { get; set; }

        /// <summary>
        /// Подсчет количества документов в каждой группе, определяемой
        /// верхней и нижней границами для значения указанного поля
        /// </summary>
        /// <example>
        /// {
        ///   "$ranges": [
        ///     { "$key": "slow", "$to": 10 },
        ///     { "$key": "medium", "$from": 10, "$to": 50 },
        ///     { "$key": "fast", "$from": 50 }
        ///   ]
        /// }
        /// </example>
        [JsonProperty("$ranges")]
        public RangeFacetExpression[] Ranges { get; set; }
    }

    /// <summary>
    /// Подсчет количества документов в каждой группе, определяемой
    /// верхней и нижней границами для значения указанного поля
    /// </summary>
    [JsonObject]
    public class RangeFacetExpression
    {
        /// <summary>
        /// Уникальное имя группы документов
        /// </summary>
        [JsonProperty("$name")]
        public string Name { get; private set; }

        /// <summary>
        /// Нижняя граница значения поля (включая указанное значение)
        /// </summary>
        [JsonProperty("$from")]
        public JValue From { get; private set; }

        /// <summary>
        /// Верхняя граница значения поля (не включая указанное значение)
        /// </summary>
        [JsonProperty("$to")]
        public JValue To { get; private set; }
    }
}