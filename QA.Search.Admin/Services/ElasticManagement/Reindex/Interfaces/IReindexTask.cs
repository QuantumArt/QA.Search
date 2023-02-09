using QA.Search.Data.Models;
using System;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    public interface IReindexTask
    {
        string SourceIndex { get; }
        string DestinationIndex { get; }
        string ElasticTaskId { get; }

        ReindexTaskStatus Status { get; }

        DateTimeOffset Created { get; }

        /// <summary>
        /// Дата и время последнего обновления
        /// </summary>
        DateTimeOffset LastUpdated { get; }
        DateTimeOffset? Finished { get; }


        TimeSpan TotalTime { get; }

        int TotalDocuments { get; }

        int CreatedDocuments { get; }
        int UpdatedDocuments { get; }
        int DeletedDocuments { get; }

        int Percentage { get; }

    }
}
