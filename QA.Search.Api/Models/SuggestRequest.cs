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
    public class SuggestRequest
    {
        /// <summary>
        /// Выбор полей документа. Поддерживает wildcards
        /// </summary>
        [JsonProperty("$select")]
        public string[] Select { get; private set; }

        /// <summary>
        /// Настройка подсвеченных фрагментов по выбранным полям.
        /// Если поля не указаны, используется псевдо-поле "_all",
        /// включающее все полнотекстовые поля документа.
        /// </summary>
        [JsonProperty("$snippets")]
        public SnippetsExpression Snippets { get; private set; }

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
        /// Минимальное количество найденных слов из <see cref="Query"/>,
        /// при котором документ попадает в выдачу. По-умолчанию необходимы ВСЕ слова.
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-minimum-should-match.html
        /// </summary>
        /// <example>
        /// 3   — должны быть найдены не менее трех слова
        /// -1  — должны быть найдены все слова кроме одного
        /// "80%" — должны быть найдены не менее 80% слов
        /// </example>
        [JsonProperty("$requiredWordsCount")]
        public JValue RequiredWordsCount { get; private set; }

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
        /// Набор условий для фильтрации контекстных полей.
        /// При отсутствии берется из <see cref="Where"/>
        /// </summary>
        [JsonProperty("$context")]
        public FilterExpression Context { get; private set; }

        /// <summary>
        /// Порядок сортировки результатов: имя поля, или путь к вложенному
        /// полю через точку или объект вида { "filed_name": "asc" | "desc" }
        /// </summary>
        /// <example>
        /// ["Region.Alias", { "Parameters.Price": "desc" }, "_score"]
        /// </example>
        [JsonProperty("$orderBy")]
        public JArray OrderBy { get; private set; }

        /// <summary>
        /// Максимальное кол-во результатов в выдаче @default 50
        /// </summary>
        [JsonProperty("$limit")]
        public int? Limit { get; private set; }

        /// <summary>
        /// Список ролей для поиска только по разрешенным ролям индексам
        /// </summary>
        [JsonProperty("$roles")]
        public string[] Roles { get; set; }

        /// <summary>
        /// Индекс запроса в массиве запросов к <see cref="Controllers.MultiSuggestController"/>
        /// </summary>
        [JsonIgnore]
        public int ArrayIndex { get; private set; }

        /// <summary>
        /// Установка query
        /// </summary>
        /// <param name="query"></param>
        public void SetQuery(string query)
        {
            Query = query;
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
        /// Десериализовать из JSON c указанием индекса запроса в массиве "multi_suggest".
        /// Выражения $from и $snippets преобразуются из сокращенных вариантов перед десериализацией.
        /// </summary>
        public static SuggestRequest FromJson(JToken json, int arrayIndex = 0)
        {
            JToken from = json["$from"];
            if (from is JValue)
            {
                json["$from"] = new JArray(from);
            }

            JToken snippets = json["$snippets"];
            if (snippets != null && snippets is JValue)
            {
                json["$snippets"] = new JObject
                {
                    ["$count"] = snippets,
                };
            }

            JToken orderBy = json["$orderBy"];
            if (orderBy != null && !(orderBy is JArray))
            {
                json["$orderBy"] = new JArray(orderBy);
            }

            try
            {
                var request = json.ToObject<SuggestRequest>();
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
                if (json["$snippets"] != snippets)
                {
                    json["$snippets"] = snippets;
                }
                if (json["$orderBy"] != orderBy)
                {
                    json["$orderBy"] = orderBy;
                }
            }
        }

        public async Task SetPresetsAsync()
        {
            var preset = await PresetsRegistry.GetSuggestPresetAsync();

            Limit = Limit ?? preset.Limit;
            if (Select == null || !Select.Any())
            {
                Select = preset.Select;
            }
        }
    }
}
