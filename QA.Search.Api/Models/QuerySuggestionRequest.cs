using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Api.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Запрос для поиска документов по префиксам слов
    /// </summary>
    public class QuerySuggestionRequest
    {
        /// <summary>
        /// Строка поискового запроса
        /// </summary>
        [JsonProperty("$query")]
        public string Query { get; private set; }

        /// <summary>
        /// Регион
        /// </summary>
        [JsonProperty("$region")]
        public string Region { get; private set; }

        /// <summary>
        /// Максимальное кол-во результатов в выдаче
        /// </summary>
        [JsonProperty("$limit")]
        public int? Limit { get; private set; }
    }
}
