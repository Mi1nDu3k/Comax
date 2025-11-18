using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly IMapper _mapper;

        public ChapterController(IChapterService chapterService, IMapper mapper)
        {
            _chapterService = chapterService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var chapters = await _chapterService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ChapterDTO>>(chapters));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null) return NotFound();
            return Ok(_mapper.Map<ChapterDTO>(chapter));
        }

        [HttpGet("comic/{comicId}")]
        public async Task<IActionResult> GetByComic(int comicId)
        {
            var chapters = await _chapterService.GetByComicIdAsync(comicId);
            return Ok(_mapper.Map<IEnumerable<ChapterDTO>>(chapters));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChapterCreateDTO dto)
        {
            var chapter = _mapper.Map<Chapter>(dto);
            await _chapterService.AddAsync(chapter);
            return CreatedAtAction(nameof(GetById), new { id = chapter.Id }, _mapper.Map<ChapterDTO>(chapter));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChapterUpdateDTO dto)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null) return NotFound();

            _mapper.Map(dto, chapter);
            await _chapterService.UpdateAsync(chapter);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null) return NotFound();

            await _chapterService.DeleteAsync(chapter);
            return NoContent();
        }
    }
}
