using Comax.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("top-comics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopComics([FromQuery] string type = "view", [FromQuery] int top = 5)
        {
            var result = await _reportService.GetTopComicsAsync(type , top);
            return Ok(result);
        }

        [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _reportService.GetDashboardStatsAsync();
            return Ok(stats);
        }

    }
}