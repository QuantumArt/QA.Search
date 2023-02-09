using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Services;

namespace QA.Search.Generic.Integration.Core.Controllers
{
    /// <summary>
    /// Generic controller for manage single ScheduledService
    /// </summary>
    [ApiController]
    public class IndexingController<TContext, TMarker> : ControllerBase
        where TContext : IndexingContext<TMarker>
        where TMarker : IServiceMarker
    {
        private readonly TContext _context;
        private readonly IServiceController<TMarker> _controller;
        private readonly ILogger _logger;

        public IndexingController(TContext context, IServiceController<TMarker> controller, ILogger<TMarker> logger)
        {
            _context = context;
            _controller = controller;
            _logger = logger;
        }

        /// <summary>
        /// Get state of indexing process
        /// </summary>
        [HttpGet]
        public TContext Get()
        {
            return _context;
        }

        /// <summary>
        /// Start indexing process
        /// </summary>
        [HttpPost("start")]
        public bool Start()
        {
            _logger.LogTrace($"Received request for start indexing {_controller}");
            return _controller.Start();
        }

        /// <summary>
        /// Stop indexing process
        /// </summary>
        [HttpPost("stop")]
        public bool Stop()
        {
            _logger.LogTrace($"Received request for stop indexing {_controller}");
            return _controller.Stop();
        }
    }
}
