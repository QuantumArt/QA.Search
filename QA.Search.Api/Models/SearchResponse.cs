using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Результат полнотекстового поиска документов
    /// с синонимами и морфологией, а также фасетного поиска
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// Копия HTTP статус-кода в теле ответа
        /// </summary>
        public int Status { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Общее количество найденных документов
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Результат исправления поисковой строки пользователя
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public QueryCorrection QueryCorrection { get; set; }

        /// <summary>
        /// Найденные документы
        /// </summary>
        public ElasticDocument[] Documents { get; set; }

        /// <summary>
        /// Словарь значений фасетов, сгруппированных по имени поля
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, FacetItem> Facets { get; set; }
    }
}