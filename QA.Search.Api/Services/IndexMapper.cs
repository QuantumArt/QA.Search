using Newtonsoft.Json.Linq;
using QA.Search.Common.Interfaces;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class IndexMapper
    {
        private readonly int _suffixLength = ".yyyy-MM-ddThh-mm-ss".Length;

        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public IndexMapper(IElasticSettingsProvider elasticSettingsProvider)
        {
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        /// <summary>
        /// Преобразует полное имя индекса Elastic вида "index.search.{name}.yyyy-MM-ddThh-mm-ss"
        /// в сокращенное имя вида "{name}", которое будет публично доступным для пользователей API
        /// </summary>
        public string ShortIndexName(string fullIndexName)
        {
            int prefixLength = _elasticSettingsProvider.GetIndexPrefix().Length;

            fullIndexName = fullIndexName[prefixLength..];

            if (fullIndexName.Length > _suffixLength)
            {
                fullIndexName = fullIndexName[0..^_suffixLength];
            }

            return fullIndexName;
        }

        /// <summary>
        /// Извлекает алиасы из ответа Elastic /_cat/aliases/{name}?format=json
        /// </summary>
        public string[] AliasesFromCatResponse(JArray body)
        {
            return body.Cast<JObject>().Select(obj => (string)obj["alias"]).ToArray();
        }

        /// <summary>
        /// Извлекает первый алиас из ответа Elastic /_cat/aliases/{name}?format=json
        /// </summary>
        public string FirstAliasFromCatResponse(JArray body)
        {
            return (string)body.Cast<JObject>().FirstOrDefault()?["alias"];
        }
    }
}