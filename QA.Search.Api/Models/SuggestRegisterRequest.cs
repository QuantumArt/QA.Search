using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Api.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
