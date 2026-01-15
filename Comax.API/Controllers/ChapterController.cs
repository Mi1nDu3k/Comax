using Comax.Business.Services;
using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs.Chapter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly IComicService _comicService;

        public ChaptersController(IChapterService chapterService, IComicService comicService)
        {
            _chapterService = chapterService;
            _comicService = comicService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("create-with-images")]
        [DisableRequestSizeLimit] 
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<IActionResult> Create([FromForm] ChapterCreateWithImagesDTO dto)
        {
            try
            {
                // Validate cơ bản
                if (dto.Images == null || dto.Images.Count == 0)
                {
                    BadRequest(SystemMessages.Chapter.ImageRequired);
                }

                var result = await _chapterService.CreateWithImagesAsync(dto);

                // Trả về kết quả 201 Created
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                // Log lỗi ra console để debug
                Console.WriteLine(ex.ToString());
                return BadRequest(new { message = ex.Message });
            }
        }
    

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChapterDTO>>> GetAll()
        {
            return Ok(await _chapterService.GetAllAsync());
        }
        [HttpGet("read/{comicSlug}/{chapterSlug}")]
        public async Task<ActionResult<ChapterDTO>> GetForReader(string comicSlug, string chapterSlug)
        {
            var chapter = await _chapterService.GetChapterBySlugsAsync(comicSlug, chapterSlug);
            if (chapter == null) return NotFound(new { message = SystemMessages.Chapter.NotFound });
            await _comicService.IncreaseViewCountAsync(chapter.ComicId);
            return Ok(chapter);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterDTO>> GetById(int id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null) return NotFound();
            await _comicService.IncreaseViewCountAsync(chapter.ComicId);
            return Ok(chapter);
        }

        [Authorize(Roles = "Admin")]
        //[HttpPost]
        //public async Task<ActionResult<ChapterDTO>> Create([FromBody] ChapterCreateDTO dto)
        //{
        //    var created = await _chapterService.CreateAsync(dto);
        //    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        //}

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ChapterDTO>> Update(int id, [FromBody] ChapterUpdateDTO dto)
        {
            var updated = await _chapterService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _chapterService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpGet("{comicSlug}/{chapterSlug}")]
        [ResponseCache(Duration = 604800)] 
        public async Task<IActionResult> GetChapter(string comicSlug, string chapterSlug)
        {
            var chapter = await _chapterService.GetChapterBySlugsAsync(comicSlug, chapterSlug);
            if (chapter == null) return NotFound();

            return Ok(chapter);
        }
    }
}