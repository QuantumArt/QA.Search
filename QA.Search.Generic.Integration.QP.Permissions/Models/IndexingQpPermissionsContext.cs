using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.QP.Permissions.Markers;
using System.Collections.Generic;

namespace QA.Search.Generic.Integration.QP.Permissions.Models
{
    public class IndexingQpPermissionsContext : IndexingContext<QpPermissionsMarker>
    {
        public List<IndexingReport> Reports { get; } = new List<IndexingReport>();
    }
}
