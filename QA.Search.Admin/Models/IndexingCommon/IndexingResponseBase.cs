using Newtonsoft.Json;
using System;
using System.Linq;

namespace QA.Search.Admin.Models.IndexingCommon
{
    public class IndexingResponseBase
    {
        protected const string DateTimeFormat = "dd.MM.yy HH:mm:ss";

        public IndexingState State { get; protected set; }

        public int Progress { get; protected set; }

        public string Message { get; protected set; }
        public int Iteration { get; protected set; }

        [JsonIgnore]
        public DateTime? StartDate { get; protected set; }

        [JsonProperty("startDate")]
        public string StartDateStr { get => StartDate?.ToString(DateTimeFormat) ?? ""; }

        [JsonIgnore]
        public DateTime? EndDate { get; protected set; }

        [JsonProperty("endDate")]
        public string EndDateStr { get => EndDate?.ToString(DateTimeFormat) ?? ""; }

        [JsonIgnore]
        public DateTime[] ScheduledDates { get; set; } = new DateTime[0];

        [JsonProperty("scheduledDates")]
        public string[] ScheduledDatesStr { get => ScheduledDates?.Select(d => d.ToString(DateTimeFormat)).ToArray() ?? new string[0]; }
    }
}
