using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Auth;
using Comax.Common.DTOs.User;
using Comax.Shared; // Giả sử chứa ErrorMessages
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
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

        // 1. Đăng ký
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.RegisterAsync(registerDto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // 2. Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            var token = await _authService.LoginAsync(request);

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

        // 3. Lấy Profile (GET) - QUAN TRỌNG: Đã thêm HttpGet
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var userId = int.Parse(userIdString);
            var user = await _userService.GetByIdAsync(userId);

            if (user == null) return NotFound(new { message = "Không tìm thấy user" });

            return Ok(user);
        }

        // 4. Cập nhật Profile (PUT) - Đã chuẩn [FromForm]
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString);

            var result = await _userService.UpdateProfileAsync(userId, request);

            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        // 5. Lấy danh sách User (Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // 6. Nâng cấp VIP
        [HttpPost("{id}/upgrade-vip")]
        [Authorize]
        public async Task<IActionResult> UpgradeToVip(int id)
        {
            var result = await _userService.UpgradeToVipAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });
            return Ok(new { message = ErrorMessages.Auth.VIPUpgradedSuccess });
        }

        // 7. Hạ cấp VIP
        [HttpPost("{id}/downgrade-vip")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DowngradeVip(int id)
        {
            var result = await _userService.DowngradeFromVipAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });
            return Ok(new { message = ErrorMessages.Auth.VIPDowngradedSuccess });
        }

        // 8. Danh sách VIP
        [HttpGet("vip-list")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> GetVipUsers()
        {
            var users = await _userService.GetVipUsersAsync();
            return Ok(users);
        }

        // 9. Ban User
        [HttpPost("{id}/ban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser(int id)
        {
            var result = await _userService.BanUserAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });
            return Ok(new { message = ErrorMessages.Auth.Banned });
        }

        // 10. Unban User
        [HttpPost("{id}/unban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var result = await _userService.UnbanUserAsync(id);
            if (!result) return NotFound(new { message = ErrorMessages.Auth.UserNotFound });
            return Ok(new { message = ErrorMessages.Auth.Unbanned });
        }

        // 11. Xóa User
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _userService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            if (!result)
            {
                // Để bảo mật, dù email không tồn tại vẫn báo thành công để tránh dò email
                return Ok(new { message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi." });
            }
            return Ok(new { message = "Vui lòng kiểm tra email để đặt lại mật khẩu." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(dto);
            if (!result)
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO dto)
        {
            // Kiểm tra tính hợp lệ của Model
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gọi Service kiểm tra xem Email + OTP có khớp trong DB không
            var isValid = await _authService.VerifyOtpAsync(dto.Email, dto.Otp);

            if (!isValid)
            {
                return BadRequest(new { message = "Mã xác thực không đúng hoặc đã hết hạn." });
            }

            // Nếu đúng, trả về OK để Frontend cho phép user nhập mật khẩu mới
            return Ok(new { message = "Mã xác thực hợp lệ." });
        }
    }
}