using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;
using QA.Search.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Api.Services
{
    // TODO: intercept field conditions from top-level $where or $context
    public class FilterTranspiler
    {
        private static readonly List<JToken> EmptyList = new(0);

        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public FilterTranspiler(IElasticSettingsProvider elasticSettingsProvider)
        {
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        /// <summary>
        /// Преобразует выражение фильтрации в "bool" query Elastic.
        /// </summary>
        public JToken BuildWhere(WhereExpression where)
        {
            var must = new List<JToken>();
            var should = new List<JToken>();
            var mustNot = new List<JToken>();

            VisitFilter(where, must, mustNot);

            if (where.Every != null)
            {
                foreach (JToken subFilter in where.Every.Select(BuildWhere))
                {
                    if (subFilter is JArray array)
                    {
                        must.AddRange(array);
                    }
                    else
                    {
                        must.Add(subFilter);
                    }
                }
            }
            if (where.Some != null)
            {
                foreach (JToken subFilter in where.Some.Select(BuildWhere))
                {
                    if (subFilter is JArray array)
                    {
                        should.AddRange(array);
                    }
                    else
                    {
                        should.Add(subFilter);
                    }
                }
            }
            if (where.Not != null)
            {
                var subFilter = BuildWhere(where.Not);
                if (subFilter is JArray array)
                {
                    mustNot.AddRange(array);
                }
                else
                {
                    mustNot.Add(subFilter);
                }
            }
            if (where.Exists != null)
            {
                var nested = new JObject { ["path"] = where.Exists };
                if (where.Where != null)
                {
                    nested["query"] = BuildWhere(where.Where);
                }
                else
                {
                    nested["query"] = new JObject { ["match_all"] = new JObject() };
                }
                must.Add(new JObject { ["nested"] = nested });
            }

            return Bool(must, should, mustNot);
        }

        private void VisitFilter(FilterExpression filter, List<JToken> must, List<JToken> mustNot)
        {
            foreach (var (field, condition) in filter)
            {
                if (condition.Either != null)
                {
                    must.Add(BuildEither(field, condition.Either));
                }
                else
                {
                    VisitCondition(field, condition, must, mustNot);
                }
            }
        }

        private JToken BuildEither(string field, ConditionExpression[] either)
        {
            var should = new List<JToken>();

            foreach (ConditionExpression condition in either)
            {
                var must = new List<JToken>();
                var mustNot = new List<JToken>();

                VisitCondition(field, condition, must, mustNot);

                should.Add(Bool(must, EmptyList, mustNot));
            }

            return Bool(EmptyList, should, EmptyList);
        }

        private void VisitCondition(
            string field, ConditionExpression condition, List<JToken> must, List<JToken> mustNot)
        {
            if (condition.Eq != null)
            {
                if (condition.Eq.Value != null)
                {
                    must.Add(Term(field, condition.Eq));
                }
                else
                {
                    mustNot.Add(Exists(field));
                }
            }
            if (condition.Ne != null)
            {
                if (condition.Ne.Value != null)
                {
                    mustNot.Add(Term(field, condition.Ne));
                }
                else
                {
                    must.Add(Exists(field));
                }
            }
            if (condition.In != null)
            {
                must.Add(Terms(field, condition.In));
            }
            if (condition.Any != null)
            {
                must.Add(Terms(field, condition.Any));
            }
            if (condition.None != null)
            {
                mustNot.Add(Terms(field, condition.None));
            }
            if (condition.All != null)
            {
                foreach (JValue term in condition.All)
                {
                    must.Add(Term(field, term));
                }
            }

            var range = new JObject();

            if (condition.Lt != null)
            {
                range["lt"] = condition.Lt;
            }
            if (condition.Gt != null)
            {
                range["gt"] = condition.Gt;
            }
            if (condition.Lte != null)
            {
                range["lte"] = condition.Lte;
            }
            if (condition.Gte != null)
            {
                range["gte"] = condition.Gte;
            }
            if (range.Count > 0)
            {
                must.Add(new JObject
                {
                    ["range"] = new JObject
                    {
                        [field] = range
                    }
                });
            }
        }

        /// <summary>
        /// Преобразум три списка выражений в "bool" query.
        /// Если какие-то списки пустые или содержат один элемент, то пытаемся сократить выражение.
        /// </summary>
        private static JToken Bool(List<JToken> must, List<JToken> should, List<JToken> mustNot)
        {
            if (should.Count == 0 && mustNot.Count == 0)
            {
                switch (must.Count)
                {
                    case 0: return new JObject();
                    case 1: return must[0];
                    default: return new JArray(must);
                }
            }

            var query = new JObject();

            if (must.Count == 1)
            {
                query["must"] = must[0];
            }
            else if (must.Count > 1)
            {
                query["must"] = new JArray(must);
            }

            if (should.Count == 1)
            {
                query["should"] = should[0];
            }
            else if (should.Count > 1)
            {
                query["should"] = new JArray(should);
            }

            if (mustNot.Count == 1)
            {
                query["must_not"] = mustNot[0];
            }
            else if (mustNot.Count > 1)
            {
                query["must_not"] = new JArray(mustNot);
            }

            return new JObject
            {
                ["bool"] = query
            };
        }

        private static JObject Exists(string field)
        {
            return new JObject
            {
                ["exists"] = new JObject
                {
                    ["field"] = field
                }
            };
        }

        private JObject Term(string field, JValue value)
        {
            if (field == "_index")
            {
                if (value.Value is string name)
                {
                    name = _elasticSettingsProvider.GetIndexPrefix() + name;

                    if (!name.Contains('*'))
                    {
                        name += ".*";
                    }

                    value.Value = name;
                }
            }
            return new JObject
            {
                ["term"] = new JObject
                {
                    [field] = value
                }
            };
        }

        private JObject Terms(string field, JValue[] values)
        {
            if (field == "_index")
            {
                foreach (JValue value in values)
                {
                    if (value.Value is string name)
                    {
                        name = _elasticSettingsProvider.GetIndexPrefix() + name;

                        if (!name.Contains('*'))
                        {
                            name += ".*";
                        }

                        value.Value = name;
                    }
                }
            }
            return new JObject
            {
                ["terms"] = new JObject
                {
                    [field] = new JArray(values)
                }
            };
        }
    }
}