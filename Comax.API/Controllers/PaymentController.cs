using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
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

            string mySePayApiToken = _config["SePay:ApiToken"]; 
            string receivedToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(mySePayApiToken) || receivedToken != mySePayApiToken)
            {
                _logger.LogWarning(SystemMessages.Payment.InvalidToken);
               return Ok();
            }

            
            if (payload.TransferType?.ToLower() != "in")
            {
                return Ok(new { success = true, message = SystemMessages.Payment.IgnoredTransaction });
            }

           
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
                        _logger.LogInformation(string.Format(SystemMessages.Payment.LogVipActivated, userId));
                    }
                }
            }
            else
            {
                _logger.LogWarning(string.Format(SystemMessages.Payment.LogUserIdNotFound, payload.Content));
            }

            return Ok(new { success = true });
        }
    }
}