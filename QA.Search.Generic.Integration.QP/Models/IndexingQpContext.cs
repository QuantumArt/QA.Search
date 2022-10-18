using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.QP.Markers;
using System.Collections.Generic;

namespace QA.Search.Generic.Integration.QP.Models
{
    public class IndexingQpContext : IndexingContext<QpMarker>
    {
        public List<IndexingReport> Reports { get; } = new List<IndexingReport>();
    }
}
