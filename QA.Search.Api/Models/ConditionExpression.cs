using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Условие фильтрации по отдельному полю.
    /// Условия внутри одного объекта объединяются через AND.
    /// Условия внутри массива объектов <see cref="Either"/> объединяются через OR.
    /// </summary>
    /// <example>
    /// { "$in": ["moskva", "spb"] }
    /// { "$gt": "2015-01-01" }
    /// [{ "$eq": null }, { "$gt": 100, "$lte": 1000 }]
    /// </example>
    public class ConditionExpression
    {
        /// <summary>
        /// Равно
        /// </summary>
        [JsonProperty("$eq")]
        public JValue Eq { get; protected set; }

        /// <summary>
        /// Не равно
        /// </summary>
        [JsonProperty("$ne")]
        public JValue Ne { get; protected set; }

        /// <summary>
        /// Содержит одно из <seealso cref="Any"/>
        /// </summary>
        [JsonProperty("$in")]
        public JValue[] In { get; protected set; }

        /// <summary>
        /// Содержит одно из <seealso cref="In"/>
        /// </summary>
        [JsonProperty("$any")]
        public JValue[] Any { get; protected set; }

        /// <summary>
        /// Содержит все
        /// </summary>
        [JsonProperty("$all")]
        public JValue[] All { get; protected set; }

        /// <summary>
        /// Не содержит ни одного из
        /// </summary>
        [JsonProperty("$none")]
        public JValue[] None { get; protected set; }

        /// <summary>
        /// Меньше
        /// </summary>
        [JsonProperty("$lt")]
        public JValue Lt { get; protected set; }

        /// <summary>
        /// Меньше или равно
        /// </summary>
        [JsonProperty("$gt")]
        public JValue Gt { get; protected set; }

        /// <summary>
        /// Больше
        /// </summary>
        [JsonProperty("$lte")]
        public JValue Lte { get; protected set; }

        /// <summary>
        /// Больше или равно
        /// </summary>
        [JsonProperty("$gte")]
        public JValue Gte { get; protected set; }

        /// <summary>
        /// Выполняется хотя бы одно из условий на ОДНО и то же поле
        /// </summary>
        [JsonIgnore]
        public ConditionExpression[] Either { get; protected set; }
    }
}