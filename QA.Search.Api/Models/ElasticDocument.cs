using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Документ Elastic, найденный с помощью полнотекстового поиска
    /// </summary>
    public class ElasticDocument
    {
        /// <summary>
        /// Строковый идентификатор документа в Elastic
        /// </summary>
        [JsonProperty("_id"), Required]
        public string Id { get; set; }

        /// <summary>
        /// Сокращенное имя индекса, которому принадлежит документ
        /// </summary>
        [JsonProperty("_index"), Required]
        public string Index { get; set; }

        /// <summary>
        /// Мера релевантности документа в рамках запроса
        /// </summary>
        [JsonProperty("_score"), Required]
        public float Score { get; set; }

        /// <summary>
        /// Словарь подсвеченных фрагментов текста, сгруппированных по имени поля
        /// </summary>
        /// <example>
        /// {
        ///   "Title": ["В <b>Москве</b> открыли"],
        ///   "Regions.Title": ["<b>Москва</b> и область"]
        /// }
        /// </example>
        [JsonProperty("_snippets", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string[]> Snippets { get; set; }

        /// <summary>
        /// Поля документа
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}