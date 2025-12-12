using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Business.Services.Interfaces; 
using Comax.Common.DTOs.Auth; 
using Comax.Common.DTOs.User;
using Comax.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService; 


        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>
        /// 1. Đăng ký
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 2. Đăng nhập
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            // Sử dụng _authService đã inject để login
            var token = await _authService.LoginAsync(request);
            if (!ModelState.IsValid)
            { 
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            // Lấy thông tin user để trả về FE
            var user = await _userService.GetByEmailAsync(request.Email);

            return Ok(new
            {
                token = token,
                user = user
            });
        }

        /// <summary>
        /// 3. Lấy danh sách User
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// 4. Nâng cấp VIP
        /// </summary>
        [HttpPost("{id}/upgrade-vip")]
        [Authorize]
        public async Task<IActionResult> UpgradeToVip(int id)
        {
            var result = await _userService.UpgradeToVipAsync(id);

            if (!result)
                return NotFound(new { message = ErrorMessages.Auth.UserNotFound });

            return Ok(new { message = ErrorMessages.Auth.VIPUpgradedSuccess });
        }

        /// <summary>
        /// 5. Hạ cấp VIP (Admin only)
        /// </summary>
        [HttpPost("{id}/downgrade-vip")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DowngradeVip(int id)
        {
            var result = await _userService.DowngradeFromVipAsync(id);

            if (!result)
                return NotFound(new { message = ErrorMessages.Auth.UserNotFound });

            return Ok(new { message = ErrorMessages.Auth.VIPDowngradedSuccess });
        }

        /// <summary>
        /// 6. Danh sách VIP (Admin only)
        /// </summary>
        [HttpGet("vip-list")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> GetVipUsers()
        {
            var users = await _userService.GetVipUsersAsync();
            return Ok(users);
        }

        [HttpPost("{id}/ban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser(int id)
        {
            var result = await _userService.BanUserAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });

            return Ok(new { message = ErrorMessages.Auth.Banned });
        }

        [HttpPost("{id}/unban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var result = await _userService.UnbanUserAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });

            return Ok(new { message = ErrorMessages.Auth.Unbanned });
        }

        /// <summary>
        /// 7. Xóa User
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Nên thêm quyền Admin cho hành động xóa
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _userService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}