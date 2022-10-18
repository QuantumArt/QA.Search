using Newtonsoft.Json;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Условия исправления поискового запроса и результатов выдачи по исправленному запросу
    /// </summary>
    /// <example>
    /// {
    ///   "$query": { "$ifFoundLte": 10 },
    ///   "$results": { "$ifFoundLte": 5 }
    /// }
    /// </example>
    [JsonObject]
    public class CorrectionExpression
    {
        /// <summary>
        /// Исправить строку поискового запроса, если найдено не больше
        /// </summary>
        [JsonProperty("$query")]
        public CorrectionLimit Query { get; private set; }

        /// <summary>
        /// Исправить результаты выдачи, если найдено не больше
        /// </summary>
        [JsonProperty("$results")]
        public CorrectionLimit Results { get; private set; }
    }

    /// <summary>
    /// Условие исправления поискового запроса
    /// </summary>
    public class CorrectionLimit
    {
        /// <summary>
        /// Исправить, если найдено не больше
        /// </summary>
        [JsonProperty("$ifFoundLte")]
        public int IfFoundLte { get; private set; }
    }
}