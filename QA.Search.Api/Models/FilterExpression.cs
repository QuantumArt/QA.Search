using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Common.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Набор условий для фильтрации документов по разным полям.
    /// Условия внутри объекта объединяются через AND.
    /// </summary>
    /// <example>
    /// {
    ///   "Region.Alias": ["moskva", "spb"],
    ///   "PublishDate": { "$gt": "2014-12-31" }
    /// }
    /// </example>
    [JsonObject]
    public class FilterExpression : ConditionExpression, IEnumerable<KeyValuePair<string, ConditionExpression>>
    {
        /// <summary>
        /// Набор условий для фильтрации документов по разным полям
        /// </summary>
        [JsonIgnore]
        private List<KeyValuePair<string, ConditionExpression>> _fields
            = new List<KeyValuePair<string, ConditionExpression>>();

        public IEnumerator<KeyValuePair<string, ConditionExpression>> GetEnumerator()
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
        ///   "Tags": { "Group": { "Alias": "новости" } },
        ///   "Themes": { "Categories.Title": ["personal", "business"] }
        /// }
        /// ▼▼▼
        /// {
        ///   "Tags.Group.Alias": { "$eq": "новости" },
        ///   "Themes.Categories.Title": { "$in": ["personal", "business"] }
        /// }
        /// </example>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _fields = new List<KeyValuePair<string, ConditionExpression>>();

            if (_props != null)
            {
                foreach (var (name, token) in _props)
                {
                    FilterExpression expression = ToExpresion(token);

                    if (expression._fields.Count > 0)
                    {
                        foreach (var (innerName, innerCondition) in expression._fields)
                        {
                            string path = name + "." + innerName;

                            _fields.Add(path, innerCondition);
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
        /// Преобразует скалярные значения в $eq, а массивы в $in или Either
        /// </summary>
        /// <example>
        /// "новости" ▶▶▶ { "$eq": "новости" }
        /// ["personal", "business"] ▶▶▶ { "$in": ["personal", "business"] }
        /// [null, { "$gt": 10 }] ▶▶▶ [{ "$eq": null }, { "$gt": 10 }]
        /// </example>
        private static FilterExpression ToExpresion(JToken token)
        {
            if (token is JValue value)
            {
                return new FilterExpression // ConditionExpression
                {
                    Eq = value
                };
            }
            if (token is JArray array)
            {
                if (array.All(el => el is JValue val && val.Value != null))
                {
                    return new FilterExpression // ConditionExpression
                    {
                        In = array.Cast<JValue>().ToArray()
                    };
                }
                return new FilterExpression // ConditionExpression
                {
                    Either = array.Select(ToExpresion).ToArray()
                };
            }
            // FilterExpression or ConditionExpression
            return token.ToObject<FilterExpression>();
        }
    }
}