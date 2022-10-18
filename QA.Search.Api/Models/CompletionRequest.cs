using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Запрос на дополнение строки поискового ввода
    /// </summary>
    public class CompletionRequest
    {
        /// <summary>
        /// Индекс(ы) для поиска
        /// </summary>
        [JsonProperty("$from")]
        public string[] From { get; private set; }

        /// <summary>
        /// Строка поискового запроса
        /// </summary>
        [JsonProperty("$query")]
        public string Query { get; private set; }

        /// <summary>
        /// Веса полей для полнотекстового поиска
        /// </summary>
        [JsonProperty("$weights")]
        public WeightsExpression Weights { get; private set; }

        /// <summary>
        /// Набор условий для фильтрации документов
        /// </summary>
        [JsonProperty("$where")]
        public WhereExpression Where { get; private set; }

        /// <summary>
        /// Максимальное кол-во результатов в выдаче @default 10
        /// </summary>
        [JsonProperty("$limit")]
        public int? Limit { get; private set; }

        /// <summary>
        /// Список ролей для поиска только по разрешенным ролям индексам
        /// </summary>
        [JsonProperty("$roles")]
        public string[] Roles { get; set; }

        /// <summary>
        /// Индекс запроса в массиве запросов к <see cref="Controllers.MultiCompletionController"/>
        /// </summary>
        [JsonIgnore]
        public int ArrayIndex { get; private set; }

        /// <summary>
        /// Токены, выделенные из <see cref="Query"/> с помощью вызова ElasticSearch /_analyze
        /// </summary>
        [JsonIgnore]
        public string[] Tokens { get; private set; }

        /// <summary>
        /// Заполнить токены, выделенные из <see cref="Query"/>
        /// </summary>
        public void SetTokens(string[] tokens)
        {
            Tokens = tokens;
        }

        /// <summary>
        /// Установка индексов для поиска
        /// </summary>
        /// <param name="from"></param>
        public void SetFrom(params string[] from)
        {
            From = from;
        }

        /// <summary>
        /// Десериализовать из JSON c указанием индекса запроса в массиве "multi_completion".
        /// Выражение $from преобразуется из сокращенного варианта перед десериализацией.
        /// </summary>
        public static CompletionRequest FromJson(JToken json, int arrayIndex = 0)
        {
            JToken from = json["$from"];
            if (from is JValue)
            {
                json["$from"] = new JArray(from);
            }

            try
            {
                var request = json.ToObject<CompletionRequest>();
                request.ArrayIndex = arrayIndex;
                return request;
            }
            finally
            {
                // restore modifiations to simulate pure function
                if (json["$from"] != from)
                {
                    json["$from"] = from;
                }
            }
        }
    }
}
