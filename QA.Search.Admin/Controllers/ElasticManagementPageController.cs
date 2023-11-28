using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.Search.Admin.Models.ElasticManagementPage;
using QA.Search.Admin.Services.ElasticManagement;
using System.Threading.Tasks;

namespace QA.Search.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ElasticManagementPageController : Controller
    {
        private ElasticManagementService Service { get; }

        //private ILogger<ElasticManagementPageController> Logger { get; }
        private ILogger Logger { get; }

        public ElasticManagementPageController(ILogger logger,
            ElasticManagementService service)
        {
            Logger = logger;
            Service = service;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(ElasticManagementPageResponse), 200)]
        [ProducesResponseType(500)]
        public IActionResult LoadData()
        {
            return Ok(Service.LoadData());
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(CreateReindexTaskResponse), 200)]
        [ProducesResponseType(500)]
        public IActionResult CreateReindexTask(string sourceIndexFullName)
        {
            return Ok(Service.CreateReindexTask(sourceIndexFullName));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateNewIndex(string indexName)
        {
            await Service.CreateNewIndex(indexName);
            return Ok();
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteIndex(string indexFullName)
        {
            await Service.RemoveIndex(indexFullName);
            return Ok(true);
        }
    }
}
