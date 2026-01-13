
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Comax.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {

    }
}