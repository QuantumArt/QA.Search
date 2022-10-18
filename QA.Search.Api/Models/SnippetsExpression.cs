using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Настройка подсвеченных фрагментов по выбранным полям.
    /// Если поля не указаны, используется псевдо-поле "_all",
    /// включающее все полнотекстовые поля документа
    /// </summary>
    /// <example>
    /// {
    ///   "Title": { "$count": 1, "$length": 120 },
    ///   "Body": { "$count": 2, "$length": 300 }
    /// }
    /// </example>
    [JsonObject]
    public class SnippetsExpression : SnippetExpression, IEnumerable<KeyValuePair<string, SnippetExpression>>
    {
        [JsonIgnore]
        public bool HasManySnippets => _fields.Count > 0;

        [JsonIgnore]
        private List<KeyValuePair<string, SnippetExpression>> _fields
            = new List<KeyValuePair<string, SnippetExpression>>();

        public IEnumerator<KeyValuePair<string, SnippetExpression>> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _props;

        /// <summary>
        /// Преобразует все вложенные объекты в набор плоских путей
        /// https://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
        /// </summary>
        /// <example>
        /// {
        ///   "Rubric": { "Title": 2 } },
        /// }
        /// ▼▼▼
        /// {
        ///   "Rubric.Title": { "$count": 2 }
        /// }
        /// </example>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _fields = new List<KeyValuePair<string, SnippetExpression>>();

            if (_props != null)
            {
                foreach (var (name, token) in _props)
                {
                    SnippetsExpression expression = ToExpresion(token);

                    if (expression._fields.Count > 0)
                    {
                        foreach (var (innerName, innerFacet) in expression._fields)
                        {
                            string path = name + "." + innerName;

                            _fields.Add(path, innerFacet);
                        }
                    }
                    else
                    {
                        _fields.Add(name, expression);
                    }
                }

                _props = null;
            }
        }

        /// <summary>
        /// Преобразует скалярные значения в "$count"
        /// </summary>
        /// <example>
        /// 10 ▶▶▶ { "$count": 10 }
        /// </example>
        private static SnippetsExpression ToExpresion(JToken token)
        {
            if (token is JValue)
            {
                // SnippetExpression
                return new SnippetsExpression { Count = (int)token };
            }
            // SnippetsExpression or SnippetExpression
            return token.ToObject<SnippetsExpression>();
        }
    }

    /// <summary>
    /// Настройка подсвеченных фрагментов по одному полю
    /// </summary>
    public class SnippetExpression
    {
        /// <summary>
        /// Кол-во подсвеченных фрагментов по одному полю @default 5
        /// Если указан 0, то подсвечивается поле целиком.
        /// </summary>
        [JsonProperty("$count")]
        public int? Count { get; set; }

        /// <summary>
        /// Максимальная длина подвсеченного фрагмента @default 100
        /// </summary>
        [JsonProperty("$length")]
        public int? Length { get; set; }
    }
}