using Elasticsearch.Net;
using Newtonsoft.Json.Linq;
using System;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public class CheckTaskStatusResult
    {
        public CheckTaskStatusResult(StringResponse elResponse)
        {
            if (!elResponse.Success || elResponse.HttpStatusCode == 404)
            {
                TotalTime = TimeSpan.FromSeconds(0);
                TotalDocuments = 0;
                CreatedDocuments = 0;
                UpdatedDocuments = 0;
                DeletedDocuments = 0;
                Status = elResponse.HttpStatusCode == 404
                    ? TaskCheckingStatus.NotFound
                    : TaskCheckingStatus.Error;
                return;
            }

            var jO = JObject.Parse(elResponse.Body);

            var completed = jO.Value<bool>("completed");

            var resp = completed
                ? jO["response"]
                : jO["task"]["status"];

            var timedOut = completed && resp.Value<bool>("timed_out");

            if (timedOut)
            {
                Status = TaskCheckingStatus.Failed;
                return;
            }

            TotalTime = completed
                ? TimeSpan.FromMilliseconds(resp.Value<int>("took"))
                : (TimeSpan?)null;

            TotalDocuments = resp.Value<int>("total");
            CreatedDocuments = resp.Value<int>("created");
            UpdatedDocuments = resp.Value<int>("updated");
            DeletedDocuments = resp.Value<int>("deleted");
            var versionConflicts = resp.Value<int>("version_conflicts");


            int totalProceeded = CreatedDocuments + UpdatedDocuments + DeletedDocuments + versionConflicts;

            if (completed && totalProceeded == TotalDocuments)
            {
                Status = TaskCheckingStatus.Completed;
                Percentage = 100;
            }
            else
            {
                Status = TaskCheckingStatus.InProgress;
                Percentage = TotalDocuments != 0 ? totalProceeded * 100 / TotalDocuments : 100;
            }
        }

        public TimeSpan? TotalTime { get; }
        public int TotalDocuments { get; }

        public int CreatedDocuments { get; }
        public int UpdatedDocuments { get; }
        public int DeletedDocuments { get; }

        public int Percentage { get; }

        public TaskCheckingStatus Status { get; }
    }
}
