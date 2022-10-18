using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Search.Api.Infrastructure;
using QA.Search.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class ContextualFieldsMapper
    {
        private readonly Settings _settings;

        public ContextualFieldsMapper(IOptions<Settings> options)
        {
            _settings = options.Value;
        }

        /// <summary>
        /// Для каждого "контекстного" поля документа <see cref="Settings.ContextualFields"/>
        /// выбирает единственное значение, удовлетворяющее фильтрам, заданным в
        /// <see cref="SearchRequest.Context"/> или в <see cref="SearchRequest.Where"/>.
        /// </summary>
        /// <remarks>
        /// Поле документа является контекстным, если при разных условиях фильтрации для
        /// одного и того же документа Elastic необходимо выдавать разные значения этого поля.
        /// 
        /// Запрос для поиска документов также имеет поле $context, в котором указан фильтр для
        /// контекстных полей документа. Если этот фильтр явно отсутствует, его значение берется из поля $where.
        /// 
        /// Пример: поле "SearchUrl", содержащее Url страницы, который начинается с поддомена.
        /// Поддомен в свою очередь зависит от одного из регионов документа, хранящихся в массиве "Regions".
        /// 
        /// Для этого при индексации в контекстное поле добавляется массив объектов, каждый из которых
        /// содержит единственное значение контекстного поля, а также выбранные значения тех
        /// полей документа, по которым должна проходить контекстная фильтрация.
        /// 
        /// Пример:
        /// {
        ///   "SearchUrl": [
        ///     {
        ///       "SearchUrl": "http://moskva.domain.ru",
        ///       "Regions": { "Id": 123, "Alias": "moskva" }
        ///     },
        ///     {
        ///       "SearchUrl": "http://spb.domain.ru",
        ///       "Regions": { "Id": 456, "Alias": "spb" }
        ///     }
        ///   ],
        ///   "Regions": [
        ///     { "Id": 123, "Alias": "moskva" },
        ///     { "Id": 456, "Alias": "spb" }
        ///   ],
        ///   // ... other fields
        /// }
        /// 
        /// Таким образом, в каждом объекте контекстного массива ОБЯЗАТЕЛЬНО содержится поле с тем же названием,
        /// что и у исходного контекстного поля, плюс поля документа, от которых она зависит.
        /// 
        /// Далее, для всех имен полей из фильтра <see cref="FilterExpression"/>, если такое поле
        /// содержится в объектах контекстного массива, то по этому полю применяется фильтрация
        /// на application-сервере, после загрузки документа из Elastic.
        /// 
        /// Если в фильтре не указано ни одного подходящего поля, то будет выбран
        /// первый попавшийся объект контекстного массива.
        /// 
        /// Пример: если в фильтре указано
        /// {
        ///   "$where": {
        ///     "Regions.Alias": "moskva",
        ///     "Tags": ["foo", "bar"]
        ///   }
        /// }
        /// 
        /// то фильтрация будет производиться только по "Regions.Alias",
        /// т.к. "Tags" не содержится в контекстных объектах.
        /// 
        /// После нахождения единственного контекстного объекта из него выбирается значение контекстного поля.
        /// 
        /// Результат:
        /// {
        ///   "SearchUrl": "http://moskva.domain.ru",
        ///   "Regions": [
        ///     { "Id": 123, "Alias": "moskva" },
        ///     { "Id": 456, "Alias": "spb" }
        ///   ],
        ///   // ... other fields
        /// }
        /// </remarks>
        public void TransformContextualFields(JArray hits, FilterExpression contextFilter)
        {
            if (hits.Count == 0) return;

            var documentsByIndex = hits
                .Cast<JObject>()
                .GroupBy(body => (string)body["_index"], body => (JObject)body["_source"]);

            foreach (IEnumerable<JObject> documents in documentsByIndex)
            {
                foreach (string field in _settings.ContextualFields)
                {
                    // determine contextual fields by first object in array
                    if (documents.First()[field] is JArray contexts &&
                        contexts.First is JObject context &&
                        context.ContainsKey(field))
                    {
                        // determine relevant conditions by first object in array
                        PathCondition[] contextConditions = contextFilter
                            ?.Select(pair => new PathCondition
                            {
                                Path = pair.Key.Split("."),
                                Condition = pair.Value,
                            })
                            .Where(pahtCondition => context.HasElasticPath(pahtCondition.Path))
                            .ToArray() ?? Array.Empty<PathCondition>();

                        // don't use LINQ in hot loop
                        foreach (JObject document in documents)
                        {
                            MapContextualFields(document, field, contextConditions);
                        }
                    }
                }
            }
        }

        private static void MapContextualFields(JObject document, string field, PathCondition[] pathConditions)
        {
            if (document[field] is JArray contexts)
            {
                JObject foundContext = FindContext(contexts, pathConditions);

                if (foundContext != null)
                {
                    document[field] = foundContext[field];
                }
                else
                {
                    document.Remove(field);
                }
            }
        }

        private static JObject FindContext(JArray contexts, PathCondition[] pathConditions)
        {
            // don't use LINQ in hot loop
            foreach (JToken token in contexts)
            {
                if (token is JObject context && MatchConditions(context, pathConditions))
                {
                    return context;
                }
            }
            return null;
        }

        private static bool MatchConditions(JObject context, PathCondition[] pathConditions)
        {
            // don't use LINQ in hot loop
            foreach (PathCondition pathFilter in pathConditions)
            {
                string[] path = pathFilter.Path;
                ConditionExpression condition = pathFilter.Condition;

                if (condition.Eq != null && !ContainsValue(context, path, condition.Eq) ||
                    condition.Ne != null && ContainsValue(context, path, condition.Ne) ||
                    condition.In != null && !ContainsAny(context, path, condition.In) ||
                    condition.Any != null && !ContainsAny(context, path, condition.Any) ||
                    condition.All != null && !ContainsAll(context, path, condition.All) ||
                    condition.None != null && ContainsAny(context, path, condition.None) ||
                    condition.Gt != null && !HasValueGt(context, path, condition.Gt) ||
                    condition.Gte != null && !HasValueGte(context, path, condition.Gte) ||
                    condition.Lt != null && !HasValueLt(context, path, condition.Lt) ||
                    condition.Lte != null && !HasValueLte(context, path, condition.Lte) ||
                    condition.Either != null && !MatchEither(context, path, condition.Either))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool MatchEither(JObject context, string[] path, ConditionExpression[] eitherConditions)
        {
            // don't use LINQ in hot loop
            if (eitherConditions.Length == 0)
            {
                return true;
            }
            foreach (ConditionExpression condition in eitherConditions)
            {
                if ((condition.Eq == null || ContainsValue(context, path, condition.Eq)) &&
                    (condition.Ne == null || !ContainsValue(context, path, condition.Ne)) &&
                    (condition.In == null || ContainsAny(context, path, condition.In)) &&
                    (condition.Any == null || ContainsAny(context, path, condition.Any)) &&
                    (condition.All == null || ContainsAll(context, path, condition.All)) &&
                    (condition.None == null || !ContainsAny(context, path, condition.None)) &&
                    (condition.Gt == null || HasValueGt(context, path, condition.Gt)) &&
                    (condition.Gte == null || HasValueGte(context, path, condition.Gte)) &&
                    (condition.Lt == null || HasValueLt(context, path, condition.Lt)) &&
                    (condition.Lte == null || HasValueLte(context, path, condition.Lte)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsValue(JObject context, string[] path, JValue value)
        {
            // don't use LINQ in hot loop
            foreach (JToken token in context.SelectElasticPath(path))
            {
                if (token is JValue element && element.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsAny(JObject context, string[] path, JValue[] values)
        {
            // don't use LINQ in hot loop
            if (values.Length == 0)
            {
                return true;
            }
            foreach (JValue value in values)
            {
                if (ContainsValue(context, path, value))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsAll(JObject context, string[] path, JValue[] values)
        {
            // don't use LINQ in hot loop
            if (values.Length == 0)
            {
                return true;
            }
            foreach (JValue value in values)
            {
                if (!ContainsValue(context, path, value))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HasValueGt(JObject context, string[] path, JValue value)
        {
            // don't use LINQ in hot loop
            try
            {
                foreach (JToken token in context.SelectElasticPath(path))
                {
                    if (token is JValue element && element.CompareTo(value) > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool HasValueGte(JObject context, string[] path, JValue value)
        {
            // don't use LINQ in hot loop
            try
            {
                foreach (JToken token in context.SelectElasticPath(path))
                {
                    if (token is JValue element && element.CompareTo(value) >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool HasValueLt(JObject context, string[] path, JValue value)
        {
            // don't use LINQ in hot loop
            try
            {
                foreach (JToken token in context.SelectElasticPath(path))
                {
                    if (token is JValue element && element.CompareTo(value) < 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool HasValueLte(JObject context, string[] path, JValue value)
        {
            // don't use LINQ in hot loop
            try
            {
                foreach (JToken token in context.SelectElasticPath(path))
                {
                    if (token is JValue element && element.CompareTo(value) <= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private struct PathCondition
        {
            public string[] Path;
            public ConditionExpression Condition;
        }
    }
}