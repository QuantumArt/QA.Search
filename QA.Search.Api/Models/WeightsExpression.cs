using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Веса полей для полнотекстового поиска
    /// </summary>
    /// <example>
    /// {
    ///   "Title": 5,
    ///   "Body": 1,
    ///   "Groups": { "Title": 2 }
    /// }
    /// </example>
    [JsonObject]
    public class WeightsExpression : IEnumerable<KeyValuePair<string, double>>
    {
        [JsonIgnore]
        private List<KeyValuePair<string, double>> _fields = new List<KeyValuePair<string, double>>();

        public IEnumerator<KeyValuePair<string, double>> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        public void AddRange(IEnumerable<KeyValuePair<string, double>> values)
        {
            _fields.AddRange(values);
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
        ///   "Tags": { "Group": { "Alias": 10 } },
        ///   "Themes": { "Categories.Title": 5 }
        /// }
        /// ▼▼▼
        /// {
        ///   "Tags.Group.Alias": 10,
        ///   "Themes.Categories.Title": 5
        /// }
        /// </example>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _fields = new List<KeyValuePair<string, double>>();

            if (_props != null)
            {
                foreach (var (name, token) in _props)
                {
                    if (token is JValue weight)
                    {
                        _fields.Add(new KeyValuePair<string, double>(name, weight.Value<double>()));
                    }
                    else
                    {
                        var expression = token.ToObject<WeightsExpression>();

                        foreach (var (innerName, innerWeight) in expression._fields)
                        {
                            string path = name + "." + innerName;

                            _fields.Add(new KeyValuePair<string, double>(path, innerWeight));
                        }
                    }
                }

                _props = null;
            }
        }
    }
}