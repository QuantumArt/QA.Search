using Newtonsoft.Json;
using System;

namespace QA.Search.Admin.Models.ElasticManagementPage
{
    public class ElasticIndexViewModel
    {
        public string Alias { get; set; }

        public bool HasAlias { get; set; }

        public string FullName { get; set; }

        public string UIName { get; set; }

        [JsonIgnore]
        public DateTime? CreationDate { get; set; }

        [JsonProperty("creationDate")]
        public string CreationDateStr { get => CreationDate?.ToString("dd.MM.yy HH:mm:ss") ?? ""; }

        public bool Readonly { get; set; }
    }

}
