using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChapterDTO>>> GetAll()
        {
            var chapters = await _chapterService.GetAllAsync();
            return Ok(chapters);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterDTO>> GetById(int id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null) return NotFound();
            return Ok(chapter);
        }

        [HttpPost]
        public async Task<ActionResult<ChapterDTO>> Create([FromBody] ChapterCreateDTO dto)
        {
            var created = await _chapterService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ChapterDTO>> Update(int id, [FromBody] ChapterUpdateDTO dto)
        {
            var updated = await _chapterService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _chapterService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
