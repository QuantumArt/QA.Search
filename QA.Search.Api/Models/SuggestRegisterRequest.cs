using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Запрос для поиска документов по префиксам слов
    /// </summary>
    public class SuggestRegisterRequest
    {
        /// <summary>
        /// Строка поискового запроса
        /// </summary>
        [Required]
        [JsonProperty("$query")]
        public string Query { get; private set; }

        /// <summary>
        /// Регион
        /// </summary>
        [Required]
        [JsonProperty("$region")]
        public string Region { get; private set; }
    }
}
