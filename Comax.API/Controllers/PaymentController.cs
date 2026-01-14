using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Payment;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _config;

        public PaymentController(IUserService userService, ILogger<PaymentController> logger, IConfiguration config)
        {
            _userService = userService;
            _logger = logger;
            _config = config;
        }

        [HttpPost("sepay-webhook")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDTO payload)
        {

            string mySePayApiToken = _config["SePay:ApiToken"]; // Lấy từ appsettings.json
            string receivedToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(mySePayApiToken) || receivedToken != mySePayApiToken)
            {
                _logger.LogWarning("SePay Webhook: Sai Token bảo mật.");
                // return Unauthorized(); // Mẹo: Nên return Ok() giả vờ để SePay không gửi lại liên tục nếu config sai
            }

            // 2. Chỉ xử lý giao dịch tiền vào ("in")
            if (payload.TransferType?.ToLower() != "in")
            {
                return Ok(new { success = true, message = "Ignored outgoing transaction" });
            }

            // 3. Phân tích nội dung chuyển khoản (Content)
            // Quy ước nội dung: "COMAX [USER_ID]" (Ví dụ: COMAX 105)
            // Dùng Regex để tách số ID ra
            var match = Regex.Match(payload.Content ?? "", @"COMAX\s+(\d+)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                int userId = int.Parse(match.Groups[1].Value);
                decimal amount = payload.TransferAmount;

                _logger.LogInformation($"Nhận được {amount} VND từ nội dung: {payload.Content}");

                // 4. Logic nghiệp vụ (Nâng VIP)
              
                if (amount >= 2000)
                {
                    var result = await _userService.UpgradeToVipAsync(userId);
                    if (result)
                    {
                        _logger.LogInformation($"Đã kích hoạt VIP cho User ID: {userId}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Không tìm thấy User ID trong nội dung: {payload.Content}");
            }

            return Ok(new { success = true });
        }
    }
}