using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComicController : ControllerBase
    {
        private readonly IComicService _comicService;

        public ComicController(IComicService comicService)
        {
            _comicService = comicService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<ComicDTO>>> GetAll([FromQuery] PaginationParams @params)
        {
            if (@params == null) @params = new PaginationParams();
            var pagedList = await _comicService.GetAllPagedAsync(@params);
            return Ok(pagedList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ComicDTO>> GetById(int id)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();
            Response.Headers.Add("ETag", comic.RowVersion.ToString());
            return Ok(comic);
        }
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncreaseView(int id)
        {
            await _comicService.IncreaseViewCountAsync(id);
            return Ok();
        }
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ComicDTO>>> Search([FromQuery] string title)
        {
            return Ok(await _comicService.SearchByTitleAsync(title));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ComicDTO>> Create([FromBody] ComicCreateDTO dto)
        {
            var created = await _comicService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ComicDTO>> Update(int id, [FromBody] ComicUpdateDTO dto)
        {
            // 1. Kiểm tra Header If-Match
            if (!Request.Headers.TryGetValue("If-Match", out var ifMatch))
            {
                return StatusCode(428, "Precondition Required: Missing If-Match header."); // 428: Yêu cầu phải có Header
            }

            var clientVersionString = ifMatch.ToString().Replace("\"", ""); // Xóa dấu ngoặc kép nếu có
            if (!Guid.TryParse(clientVersionString, out var clientVersion))
            {
                return BadRequest("Invalid ETag format.");
            }

            try
            {
                var currentEntity = await _comicService.GetByIdAsync(id);
                if (currentEntity == null) return NotFound();

                if (currentEntity.RowVersion != clientVersion)
                {
                    return StatusCode(412, "Precondition Failed: Data has been modified by another user.");
                }

                // 3. Thực hiện update
                var updated = await _comicService.UpdateAsync(id, dto);

                // 4. Trả về ETag mới
                Response.Headers.Add("ETag", updated.RowVersion.ToString());
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _comicService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}