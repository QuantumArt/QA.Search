using QA.Search.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    public interface IReindexTask
    {
        string SourceIndex { get; }
        string DestinationIndex { get; }
        string ElasticTaskId { get; }

        ReindexTaskStatus Status { get; }

        DateTime Created { get; }
        
        /// <summary>
        /// Дата и время последнего обновления
        /// </summary>
        DateTime LastUpdated { get; }
        DateTime? Finished { get; }


        TimeSpan TotalTime { get; }

        int TotalDocuments { get; }

        int CreatedDocuments { get; }
        int UpdatedDocuments { get; }
        int DeletedDocuments { get; }

        int Percentage { get; }

    }
}
