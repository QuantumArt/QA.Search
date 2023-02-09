using Newtonsoft.Json;
using QA.Search.Api.Models.ElasticSearch;
using QA.Search.Api.Models.ElasticSearch.RequestSections;

namespace QA.Search.Api.Builders
{
    /// <summary>
    /// Билдер создания тела запроса для получения предположений
    /// </summary>
    public class EsRequestBuilder
    {
        private int? size;
        private QuerySection query;
        private AggsSection aggs;
        private object insertedObj;

        public void SetSize(int size)
        {
            this.size = size;
        }

        public void SetQuerySection(QuerySection querySection)
        {
            this.query = querySection;
        }

        public void SetAggsSection(AggsSection aggsSection)
        {
            this.aggs = aggsSection;
        }

        public void SetInsertedObj(object insertedObj)
        {
            this.insertedObj = insertedObj;
        }

        public string Build()
        {
            if (insertedObj != null)
            {
                return JsonConvert.SerializeObject(insertedObj, Common.Settings.JsonCamelCaseSerializer);
            }
            else
            {
                var result = new EsRequest
                {
                    Query = query,
                    Aggs = aggs,
                    Size = size
                };
                return JsonConvert.SerializeObject(result, Common.Settings.JsonCamelCaseSerializer);
            }
        }
    }
}
