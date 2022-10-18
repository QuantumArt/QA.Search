using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Результат поиска документов по префиксам слов в текстах
    /// </summary>
    public class SuggestResponse
    {
        /// <summary>
        /// Копия HTTP статус-кода в теле ответа
        /// </summary>
        public int Status { get; set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Найденные документы
        /// </summary>
        [Required]
        public ElasticDocument[] Documents { get; set; }
    }
}