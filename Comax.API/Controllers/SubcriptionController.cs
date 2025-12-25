using Comax.Business.Interfaces;
using Comax.Common.DTOs.Subscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Comax.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("recharge")]
        public async Task<IActionResult> RechargeVip([FromBody] UpgradeVipRequest request)
        {
            // 1. Lấy UserId từ Token đã đăng nhập
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            // 2. Kiểm tra tính hợp lệ của số tháng
            if (request.Months <= 0) return BadRequest("Số tháng không hợp lệ.");

            // 3. Gọi Service xử lý nâng cấp VIP và tính ngày hết hạn
            var result = await _subscriptionService.ProcessVipUpgradeAsync(userId, request.Months);

            if (result)
            {
                return Ok(new { message = "Gia hạn VIP thành công!" });
            }

            return BadRequest("Không thể thực hiện gia hạn.");
        }
    }
}