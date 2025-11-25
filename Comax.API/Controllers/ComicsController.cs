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
            var pagedList = await _comicService.GetAllPagedAsync(@params);
            return Ok(pagedList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ComicDTO>> GetById(int id)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();
            return Ok(comic);
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
            var updated = await _comicService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
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