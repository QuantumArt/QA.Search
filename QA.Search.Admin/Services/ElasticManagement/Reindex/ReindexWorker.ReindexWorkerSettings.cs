using System;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public class ReindexWorkerSettings
    {
        public bool RunTasks { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
