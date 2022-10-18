using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public class ReindexWorkerSettings
    {
        public bool RunTasks { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
