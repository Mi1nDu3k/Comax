using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Shared; // <-- Import namespace Shared
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

        /// <summary>
        /// 1. Đăng ký
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            // Validator đã tự động chạy (nhờ cấu hình trong Program.cs)
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
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 3. Lấy danh sách (Có thể thêm phân trang sau này)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// 4. Nâng cấp VIP
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/upgrade-vip")]
        [Authorize]
        public async Task<IActionResult> UpgradeToVip(int id)
        {
            var result = await _userService.UpgradeToVipAsync(id);

            if (!result)
                return NotFound(new { message = ErrorMessages.Auth.UserNotFound }); // Refactor

            return Ok(new { message = ErrorMessages.Auth.VIPUpgradedSuccess }); // Refactor
        }

        /// <summary>
        /// 5. Hạ cấp VIP (Admin only)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/downgrade-vip")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DowngradeVip(int id)
        {
            var result = await _userService.DowngradeFromVipAsync(id);

            if (!result)
                return NotFound(new { message = ErrorMessages.Auth.UserNotFound }); // Refactor

            return Ok(new { message = ErrorMessages.Auth.VIPDowngradedSuccess }); // Refactor
        }

        /// <summary>
        /// 6. Danh sách VIP (Admin only)
        /// </summary>
        /// <returns></returns>
        [HttpGet("vip-list")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> GetVipUsers()
        {
            var users = await _userService.GetVipUsersAsync();
            return Ok(users);
        }


        [HttpPost("{id}/ban")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được khóa
        public async Task<IActionResult> BanUser(int id)
        {
            var result = await _userService.BanUserAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });

            // Bạn có thể thêm message vào ErrorMessages.Auth.BannedSuccess nếu muốn
            return Ok(new { message = ErrorMessages.Auth.Banned});
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
        /// <param name="id"></param>
        /// <param name="hardDelete"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _userService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}