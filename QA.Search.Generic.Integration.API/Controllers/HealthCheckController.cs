using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;

namespace QA.Search.Generic.Integration.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IElasticLowLevelClient _elasticClient;

        public HealthCheckController(IElasticLowLevelClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        // GET api/healthcheck
        [HttpGet]
        public string Index()
        {
            var response = _elasticClient.Cat.Health<StringResponse>();
            if (response.Success)
            {
                return "OK: " + response.Body;
            }
            else
            {
                return "Error: " + response.DebugInformation;
            }
        }
    }
}