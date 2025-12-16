using Microsoft.AspNetCore.Mvc;

[Route("api/notifications")] 
[ApiController]
public class NotificationController : ControllerBase
{
    [HttpGet]
    public IActionResult GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        return Ok(new
        {
            Data = new List<object>(),
            TotalCount = 0
        });
    }
}