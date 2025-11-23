using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
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
        public async Task<ActionResult<IEnumerable<ComicDTO>>> GetAll()
        {
            var comics = await _comicService.GetAllAsync();
            return Ok(comics);
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
            var comics = await _comicService.SearchByTitleAsync(title);
            return Ok(comics);
        }

        [HttpPost]
        public async Task<ActionResult<ComicDTO>> Create([FromBody] ComicCreateDTO dto)
        {
            var created = await _comicService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ComicDTO>> Update(int id, [FromBody] ComicUpdateDTO dto)
        {
            var updated = await _comicService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _comicService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
