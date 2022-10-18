using Newtonsoft.Json;
using System;
using System.Linq;

namespace QA.Search.Api.JsonConverters
{
    /// <summary>
    /// Кастомный конвертер массива в множественное условие эластика.
    /// Преобразует массив строк в строку где элементы массива отделены словом OR.
    /// </summary>
    public class RolesToElasticQueryConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string[] loweredRoles = ((string[])value).Select(x => x.ToLowerInvariant()).ToArray();
            writer.WriteValue(string.Join(" OR ", loweredRoles));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string[]);
        }
    }
}
