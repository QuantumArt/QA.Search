using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Search.Admin.Errors;
using QA.Search.Admin.Models;
using QA.Search.Admin.Services;
using System;
using System.Threading.Tasks;

namespace QA.Search.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AccountController : Controller
    {
        private readonly AuthenticationService _authenticationService;
        private readonly UsersService _usersService;

        public AccountController(AuthenticationService authenticationService, UsersService usersService)
        {
            _authenticationService = authenticationService;
            _usersService = usersService;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(BusinessError), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequest data)
        {
            await _authenticationService.Login(HttpContext, data.Email, data.Password);

            return Ok();
        }

        [Authorize]
        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            await _authenticationService.Logout(HttpContext);

            return Ok();
        }

        [Authorize]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        public async Task<IActionResult> Info()
        {
            var user = await _usersService.GetUser(User.Identity.Name);

            if (user == null)
            {
                await _authenticationService.Logout(HttpContext);
                return BadRequest();
            }

            return Ok(user);
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> SendResetPasswordLink([FromBody] ResetPasswordRequest data)
        {
            await _usersService.SendResetPasswordLink(data.Email);

            return Ok();
        }

        [HttpGet("CheckResetPasswordLink/{id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> CheckResetPasswordLink([FromRoute] Guid id)
        {
            var user = await _usersService.GetUserByResetPasswordLinkId(id);

            return Ok(user);
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest data)
        {
            await _usersService.ChangePassword(data.EmailId, data.Password);

            return Ok();
        }
    }
}
