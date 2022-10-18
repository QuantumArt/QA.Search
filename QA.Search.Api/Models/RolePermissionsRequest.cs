using Newtonsoft.Json;
using QA.Search.Api.JsonConverters;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Класс запроса индексов по ролям в эластик.
    /// </summary>
    public class RolePermissionsRequest
    {
        [JsonProperty("query")]
        public RolePermissionsQuery Query { get; } = new RolePermissionsQuery();
    }

    /// <summary>
    /// Класс прослойка для сериализации без извратов.
    /// </summary>
    public class RolePermissionsQuery
    {
        [JsonProperty("query_string")]
        public RolePermissionsSubquery Subquery { get; } = new RolePermissionsSubquery();
    }

    /// <summary>
    /// Класс с данными для запроса индексов по ролям из индекса эластика.
    /// </summary>
    public class RolePermissionsSubquery
    {
        /// <summary>
        /// Поле обозначающее в каком столбце искать.
        /// </summary>
        [JsonProperty("fields")]
        public string[] Fields { get; } = { "Role" };

        /// <summary>
        /// Массив ролей которые требуется найти. Обрабатывается кастомным конвертером.
        /// </summary>
        [JsonProperty("query"), JsonConverter(typeof(RolesToElasticQueryConverter))]
        public string[] Roles { get; set; }
    }
}
