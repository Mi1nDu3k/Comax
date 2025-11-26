using Comax.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Chỉ Admin mới xem được báo cáo
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _reportService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("top-comics")]
        public async Task<IActionResult> GetTopComics([FromQuery] string type = "view", [FromQuery] int top = 5)
        {
            // type: "view" hoặc "rating"
            var result = await _reportService.GetTopComicsAsync(type, top);
            return Ok(result);
        }
    }
}