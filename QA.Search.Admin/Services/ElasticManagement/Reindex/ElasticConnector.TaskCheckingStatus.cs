using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public enum TaskCheckingStatus
    {
        NotFound,
        Completed,
        Failed,
        InProgress,
        Error
    }
}
