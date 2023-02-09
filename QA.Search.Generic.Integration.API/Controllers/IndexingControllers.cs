using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.Search.Generic.Integration.Core.Controllers;
using QA.Search.Generic.Integration.Core.Services;
using QA.Search.Generic.Integration.QP.Markers;
using QA.Search.Generic.Integration.QP.Models;

namespace QA.Search.Generic.Integration.API.Controllers
{
    [Route("api/qp/indexing")]
    public class IndexingQPController : IndexingController<IndexingQpContext, QpMarker>
    {
        public IndexingQPController(
            IndexingQpContext context,
            IServiceController<QpMarker> controller,
            ILogger<QpMarker> logger)
            : base(context, controller, logger)
        {
        }
    }

    [Route("api/qp/indexing/update")]
    public class IndexingQPUpdateController : IndexingController<IndexingQpUpdateContext, QpUpdateMarker>
    {
        public IndexingQPUpdateController(
            IndexingQpUpdateContext context,
            IServiceController<QpUpdateMarker> controller,
            ILogger<QpUpdateMarker> logger)
            : base(context, controller, logger)
        {
        }
    }
}