using Comax.Business.Interfaces;
using Comax.Common.Enums;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public SubscriptionService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<bool> ProcessVipUpgradeAsync(int userId, int months)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

           
            user.IsVip = true;
            user.SubStatus = SubscriptionStatus.Active;

            
            DateTime startDate = (user.VipExpireAt.HasValue && user.VipExpireAt.Value > DateTime.UtcNow)
                ? user.VipExpireAt.Value
                : DateTime.UtcNow;

            user.VipExpireAt = startDate.AddMonths(months);

          
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

          
            await _notificationService.CreateAndSendNotificationAsync(
                userId,
                $"Nâng cấp VIP thành công! Thời hạn đến: {user.VipExpireAt:dd/MM/yyyy}",
                "/profile"
            );

            return true;
        }
    }
}