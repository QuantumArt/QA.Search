using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Search.Admin.Errors;
using QA.Search.Admin.Models;
using QA.Search.Admin.Models.QpIndexingPage;
using QA.Search.Admin.Services;
using QA.Search.Admin.Services.IndexingApi;

namespace QA.Search.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class QpIndexingController : Controller
    {
        private QpIndexingApiService IndexingApiService { get; }

        public QpIndexingController(QpIndexingApiService indexingApiService)
        {
            IndexingApiService = indexingApiService;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(QpIndexingResponse), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetIndexingStatus(TargetQP targetQP)
        {
            return Ok(await IndexingApiService.GetIndexingData(targetQP));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> StartIndexing(TargetQP targetQP)
        {
            await IndexingApiService.StartIndexing(targetQP);
            return Ok();
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> StopIndexing(TargetQP targetQP)
        {
            await IndexingApiService.StopIndexing(targetQP);
            return Ok();
        }
    }
}
