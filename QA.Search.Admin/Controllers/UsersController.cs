using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Search.Admin.Models;
using QA.Search.Admin.Services;
using System.Threading.Tasks;

namespace QA.Search.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [IgnoreAntiforgeryToken]
    public class UsersController : Controller
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UsersListResponse), 200)]
        public async Task<IActionResult> ListUsers([FromQuery] UsersListRequest request)
        {
            var response = await _usersService.ListUsers(request);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            await _usersService.DeleteUser(id);

            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest data)
        {
            await _usersService.CreateUser(data.Email, data.Role);

            return Ok();
        }
    }
}
