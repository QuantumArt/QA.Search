using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace QA.Search.Api.Infrastructure
{
    /// <summary>
    /// ElasticSearch хранит массивы объектов в плоской структуре.
    /// Поэтому ко всем встреченным по пути массивам объектов применяется операция 'flatten'.
    /// https://www.elastic.co/guide/en/elasticsearch/reference/current/array.html
    /// </summary>
    public static class ElasticJsonExtensions
    {
        /// <summary>
        /// Получить список значений поля документа по заданному пути.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="path">Список имен полей документа</param>
        public static IEnumerable<JToken> SelectElasticPath(this JObject document, params string[] path)
        {
            return SelectElasticPath(document, path, level: 0);
        }

        private static IEnumerable<JToken> SelectElasticPath(JToken node, string[] path, int level)
        {
            while (level < path.Length && node is JObject obj)
            {
                node = obj[path[level++]];
            }
            if (node != null)
            {
                if (level == path.Length)
                {
                    if (node is JArray arr)
                    {
                        foreach (JToken token in arr)
                        {
                            yield return token;
                        }
                    }
                    else
                    {
                        yield return node;
                    }
                }
                else if (node is JArray arr && arr.First is JObject)
                {
                    foreach (JToken item in arr)
                    {
                        foreach (JToken token in SelectElasticPath(item, path, level))
                        {
                            yield return token;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверить, содержит ли документ указанный путь
        /// </summary>
        /// <param name="document"></param>
        /// <param name="path">Список имен полей документа</param>
        public static bool HasElasticPath(this JObject document, params string[] path)
        {
            return HasElasticPath(document, path, level: 0);
        }

        private static bool HasElasticPath(JToken node, string[] path, int level)
        {
            while (level < path.Length && node is JObject obj)
            {
                node = obj[path[level++]];
            }
            if (node != null)
            {
                if (level == path.Length)
                {
                    return true;
                }
                if (node is JArray arr && arr.First is JObject)
                {
                    foreach (JToken item in arr)
                    {
                        if (HasElasticPath(item, path, level))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}