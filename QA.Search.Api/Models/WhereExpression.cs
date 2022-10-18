using Newtonsoft.Json;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Дерево выражений для комбинирования условий с помощью булевых операторов.
    /// </summary>
    /// <example>
    /// {
    ///   "$some": [{
    ///     "Region.Alias": ["moskva", "spb"],
    ///     "PublishDate": { "$gt": "2014-12-31" }
    ///   }, {
    ///     "Region.Alias": "vladivostok",
    ///     "PublishDate": { "$gt": "2015-01-01" }
    ///   }],
    ///   "$exists": "MarketingProduct.Parameters",
    ///   "$where": {
    ///     "BaseParameter.Alias": "price",
    ///     "NumValue": { "$lt": 1000 }
    ///   }
    /// }
    /// </example>
    public class WhereExpression : FilterExpression
    {
        /// <summary>
        /// Выполняются все указанные условия
        /// </summary>
        [JsonProperty("$every")]
        public WhereExpression[] Every { get; private set; }

        /// <summary>
        /// Выполняется хотя бы одно из указанных условий
        /// </summary>
        [JsonProperty("$some")]
        public WhereExpression[] Some { get; private set; }

        /// <summary>
        /// Не выполняется указанное условие
        /// </summary>
        [JsonProperty("$not")]
        public WhereExpression Not { get; private set; }

        /// <summary>
        /// По указанному пути содержится хотя бы один 'nested' документ
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-nested-query.html
        /// </summary>
        [JsonProperty("$exists")]
        public string Exists { get; private set; }

        /// <summary>
        /// Условия фильтрации для 'nested' документа. Bсе имена полей в условии должны
        /// иметь префикс, указанный в <see cref="Exists"/>
        /// </summary>
        [JsonProperty("$where")]
        public WhereExpression Where { get; private set; }
    }
}