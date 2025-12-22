using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> ProcessVipUpgradeAsync(int userId, int months);
    }
}
