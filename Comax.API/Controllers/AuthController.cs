using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // -----------------------------
        // LOGIN
        // -----------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(model);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }

        // -----------------------------
        // REGISTER
        // -----------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
