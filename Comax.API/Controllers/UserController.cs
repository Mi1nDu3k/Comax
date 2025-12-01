using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gọi hàm từ UserService
            var result = await _userService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gọi hàm từ UserService
            var result = await _userService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpPost("{id}/upgrade-vip")]
        [Authorize]
        public async Task<IActionResult> UpgradeToVip(int id)
        {
            var result = await _userService.UpgradeToVipAsync(id);

            if (!result) return NotFound(ErrorMessages.User.UserNotFound);

            return Ok(new { message = "VIP account upgraded successfully! Please log in again to update your privileges." });
        }

        [HttpPost("{id}/downgrade-vip")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DowngradeVip(int id)
        {
            var result = await _userService.DowngradeFromVipAsync(id);

            if (!result) return NotFound("User not found");

            return Ok(new { message = "The account has been downgraded to a regular member." });
        }

        [HttpGet("vip-list")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> GetVipUsers()
        {
            var users = await _userService.GetVipUsersAsync();
            return Ok(users);
        }
        //Delete với hardDelete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _userService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}