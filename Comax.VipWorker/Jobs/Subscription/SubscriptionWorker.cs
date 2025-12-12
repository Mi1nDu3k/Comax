using Comax.Common.Enums;
using Comax.Data;
using Comax.VipWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Comax.VipWorker.Jobs.Subscription
{
    public class SubscriptionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionWorker> _logger;
        // Quét mỗi 1 giờ (có thể chỉnh thấp hơn để test)
        //private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));

        private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));

        public SubscriptionWorker(IServiceProvider serviceProvider, ILogger<SubscriptionWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Job started...");

            // Vòng lặp quét
            while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSubscriptionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Subscription Job");
                }
            }
        }

        private async Task ProcessSubscriptionsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ComaxDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.UtcNow;
            var fiveDaysLater = now.AddDays(5);

            ///summary
            ///CASE 1: XỬ LÝ SẮP HẾT HẠN(ExpiredSoon) - Còn 5 ngày
            ///Điều kiện: Là VIP +Có ngày hết hạn <= 5 ngày tới +Chưa hết hạn +Status chưa phải là ExpiredSoon
            ///summary/
            var usersExpiredSoon = await context.Users
                .Where(u => u.IsVip
                            && u.VipExpireAt.HasValue
                            && u.VipExpireAt.Value <= fiveDaysLater
                            && u.VipExpireAt.Value > now
                            && u.SubStatus != SubscriptionStatus.ExpiredSoon)
                .ToListAsync(stoppingToken);

            foreach (var user in usersExpiredSoon)
            {
        
                user.SubStatus = SubscriptionStatus.ExpiredSoon;

         
                await emailService.SendEmailAsync(
                    user.Email,
                    "Cảnh báo: VIP sắp hết hạn!",
                    $"Chào {user.Username}, gói VIP của bạn sẽ hết hạn vào {user.VipExpireAt}. Hãy gia hạn ngay!"
                );
            }

            /// Summary
            /// CASE 2: XỬ LÝ ĐÃ HẾT HẠN (Expired)
            /// Điều kiện: Là VIP + Ngày hết hạn nhỏ hơn hiện tại
            ///summary/
            var usersExpired = await context.Users
                .Where(u => u.IsVip
                            && u.VipExpireAt.HasValue
                            && u.VipExpireAt.Value <= now)
                .ToListAsync(stoppingToken);

            foreach (var user in usersExpired)
            {
                
                user.SubStatus = SubscriptionStatus.Expired;
                user.IsVip = false;
                user.VipExpireAt = null; 

                
                await emailService.SendEmailAsync(
                    user.Email,
                    "Thông báo: VIP đã hết hạn",
                    $"Chào {user.Username}, gói VIP của bạn đã hết hạn. Bạn đã trở về tài khoản thường."
                );
            }

            // Lưu thay đổi vào DB nếu có bất kỳ user nào được xử lý
            if (usersExpiredSoon.Any() || usersExpired.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Processed: {usersExpiredSoon.Count} ExpiredSoon, {usersExpired.Count} Expired.");
            }
        }
    }
}