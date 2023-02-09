using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Search.Api.Services;
using QA.Search.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Api.Controllers
{
    /// <summary>
    /// Получение схемы полей иидексов Elastic для отображения
    /// во встроенном редакторе запросов Search.Api
    /// </summary>
    [Route("api/v1/mapping")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MappingController : ElasticController
    {
        private readonly IndexMapper _indexMapper;
        private readonly IElasticSettingsProvider _elasticSettingsProvider;

        public MappingController(
            IOptions<Settings> options,
            ILogger<MappingController> logger,
            IElasticLowLevelClient elastic,
            IndexMapper indexMapper,
            IElasticSettingsProvider elasticSettingsProvider)
            : base(options, logger, elastic)
        {
            _indexMapper = indexMapper;
            _elasticSettingsProvider = elasticSettingsProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Mapping()
        {
            var mappingResponse = await _elastic.Indices.GetMappingAsync<StringResponse>(_elasticSettingsProvider.GetAliasMask());

            if (!mappingResponse.Success)
            {
                return ElasticError(mappingResponse);
            }

            IDictionary<string, JToken> indexMappings = JObject.Parse(mappingResponse.Body);

            var indexSchemas = indexMappings.ToDictionary(
                prop => _indexMapper.ShortIndexName(prop.Key),
                prop => MapMappings((JObject)prop.Value["mappings"]));

            return Ok(indexSchemas);
        }

        /// <summary>
        /// Map first type from "mappings" object
        /// </summary>
        private JObject MapMappings(JObject mappings)
        {
            JObject schema = MapObject(mappings.PropertyValues().FirstOrDefault() as JObject);

            // replace "Contextual Fields" object mappings by it's output field mapping
            foreach (string field in _settings.ContextualFields)
            {
                if (schema[field] is JObject context && context[field] is JToken valueSchema)
                {
                    schema[field] = valueSchema;
                }
            }

            return schema;
        }

        private JObject MapObject(JObject objectMapping)
        {
            var objectSchema = new JObject();

            if (objectMapping == null)
                return objectSchema;

            foreach (var (key, valueMapping) in (JObject)objectMapping["properties"])
            {
                var valueSchema = MapValue((JObject)valueMapping);

                if (!key.StartsWith("_") && valueSchema != null)
                {
                    objectSchema[key] = valueSchema;
                }
            }

            return objectSchema;
        }

        private JToken MapValue(JObject valueMapping)
        {
            var type = valueMapping["type"];
            var fields = valueMapping["fields"];
            var properties = valueMapping["properties"];

            if (properties != null)
            {
                if (type != null && type.Value<string>() == "nested")
                {
                    return new JArray(MapObject(valueMapping));
                }
                return MapObject(valueMapping);
            }
            if (fields != null && fields["synonyms"] != null && fields["shingles"] != null)
            {
                return "text";
            }
            if (type != null)
            {
                switch (type.Value<string>())
                {
                    case "date":
                        return "date";

                    case "text":
                    case "keyword":
                    case "binary":
                    case "ip":
                        return "string";

                    case "long":
                    case "integer":
                    case "short":
                    case "byte":
                    case "double":
                    case "float":
                    case "half_float":
                    case "scaled_float":
                        return "number";

                    case "boolean":
                        return "boolean";

                    case "integer_range":
                    case "float_range":
                    case "long_range":
                    case "double_range":
                        return new JObject { ["gte"] = "number", ["lte"] = "number" };

                    case "date_range":
                        return new JObject { ["gte"] = "date", ["lte"] = "date" };

                    case "ip_range":
                        return new JObject { ["gte"] = "string", ["lte"] = "string" };
                }
            }

            return null;
        }
    }
}