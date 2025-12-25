
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Comax.Business.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Có thể map UserId với ConnectionId tại đây
            await base.OnConnectedAsync();
        }
    }
}