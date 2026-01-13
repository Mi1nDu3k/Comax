using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Comax.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();
            var result = await _historyService.GetHistoryAsync(userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveHistory([FromBody] HistoryCreateDTO dto)
        {
            var userId = GetCurrentUserId();
            await _historyService.AddOrUpdateHistoryAsync(userId, dto);
            return Ok(new { message = "Saved successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            await _historyService.DeleteHistoryAsync(userId, id);
            return Ok(new { message = "Deleted" });
        }
        [HttpDelete] 
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetCurrentUserId();
            await _historyService.DeleteAllHistoryAsync(userId);
            return Ok(new { message = "All histories are deleted." });
        }
        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Token invalid");
        }
    }
}