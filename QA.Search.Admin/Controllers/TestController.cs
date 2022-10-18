using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.Search.Admin.Errors;
using QA.Search.Admin.Models;
using QA.Search.Admin.Models.ElasticManagementPage;
using QA.Search.Admin.Services.ElasticManagement;
using QA.Search.Admin.Services.ElasticManagement.Reindex;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement;

namespace QA.Search.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class TestController : Controller
    {
        private ReindexTaskManager TaskManager { get; }

        private ElasticConnector ElasticConnector { get; }

        public TestController(ReindexTaskManager taskManager, ElasticConnector elasticConnector)
        {
            TaskManager = taskManager;
            ElasticConnector = elasticConnector;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(IEnumerable<IReindexTask>), 200)]
        [ProducesResponseType(500)]
        public IActionResult GetFinishedTasks()
        {
            var data = TaskManager.GetFinishedTasks();
           return Ok(data);
        }
    }
}
