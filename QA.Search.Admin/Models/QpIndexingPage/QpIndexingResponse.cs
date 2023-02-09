using QA.Search.Admin.Models.IndexingCommon;
using QA.Search.Generic.Integration.QP.Models;
using System.Linq;

namespace QA.Search.Admin.Models.QpIndexingPage
{
    public class QpIndexingResponse : IndexingResponseBase
    {
        public QpIndexingResponse(IndexingQpContext context)
        {
            State = (IndexingState)(int)context.State;
            Progress = context.Progress;
            Message = context.Message;
            Iteration = context.Iteration;
            StartDate = context.StartDate;
            EndDate = context.EndDate;
            ScheduledDates = context.ScheduledDates;

            Reports = context.Reports?.Select(r => new IndexingReportModel(r)).ToArray() ?? new IndexingReportModel[0];
        }

        public IndexingReportModel[] Reports { get; set; } = new IndexingReportModel[0];
    }
}
