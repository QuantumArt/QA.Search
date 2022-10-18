using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QA.Search.Api.Models;
using QA.Search.Api.Models.ElasticSearch;
using QA.Search.Api.Models.ElasticSearch.RequestSections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Builders
{
    public class MultiMatchSectionBuilder
    {
        private string query;
        private string minimumShouldMatch;
        private List<string> fields;
        private string type;

        public void SetQuery(string query)
        {
            this.query = query;
        }

        public void SetMinimumShouldMatch(string minimumShouldMatch)
        {
            this.minimumShouldMatch = minimumShouldMatch;
        }

        public void AddField(string name)
        {
            fields = fields ?? new List<string>();
            fields.Add(name);
        }

        public void SetType(string type)
        {
            this.type = type;
        }

        public MultiMatchSection Build()
        {
            var result = new MultiMatchSection
            {
                Query = query
            };

            if (fields == null)
            {
                fields = new List<string> { "_prefixes" };
            }
            result.Fields = fields;

            if (string.IsNullOrEmpty(type))
            {
                type = "best_fields";
            }
            result.Type = type;

            if (!string.IsNullOrEmpty(minimumShouldMatch))
            {
                result.MinimumShouldMatch = minimumShouldMatch;
            }
            else
            {
                result.Operator = "and";
            }

            return result;
        }
    }
}
