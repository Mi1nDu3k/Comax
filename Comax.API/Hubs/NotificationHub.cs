using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Comax.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Lấy UserID từ Token
            var userId = Context.User.FindFirst("id")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Gom User vào Group riêng: "User_1", "User_2"...
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }
    }
}