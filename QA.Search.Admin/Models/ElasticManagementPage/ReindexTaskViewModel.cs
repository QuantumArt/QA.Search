using Newtonsoft.Json;
using QA.Search.Data.Models;
using System;

namespace QA.Search.Admin.Models.ElasticManagementPage
{
    /// <summary>
    /// Модель процесса бесшовной переиндексации одного индекса Elastic
    /// </summary>
    public class ReindexTaskViewModel
    {
        public string SourceIndex { get; set; }
        public string DestinationIndex { get; set; }
        [JsonIgnore]
        public DateTime Created { get; set; }
        [JsonProperty("created")]
        public string CreatedStr { get => Created.ToString("dd.MM.yy HH:mm:ss"); }
        [JsonIgnore]
        public DateTime? Finished { get; set; }
        [JsonProperty("finished")]
        public string FinishedStr { get => Finished?.ToString("dd.MM.yy HH:mm:ss") ?? ""; }
        public ReindexTaskStatus Status { get; set; }
        [JsonIgnore]
        public DateTime LastUpdated { get; set; }
        [JsonProperty("lastUpdated")]
        public string LastUpdatedStr { get => LastUpdated.ToString("dd.MM.yy HH:mm:ss") ; }

        [JsonIgnore]
        public TimeSpan TotalTime { get; set; }
        [JsonProperty("totalTime")]
        public string TotalTimeStr { get => TotalTime.ToString(); }
        public int TotalDocuments { get; set; }
        public int CreatedDocuments { get; set; }
        public int UpdatedDocuments { get; set; }
        public int DeletedDocuments { get; set; }
        public int Percentage { get; set; }
    }

}
