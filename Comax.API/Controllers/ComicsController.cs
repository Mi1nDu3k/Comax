using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs.Chapter;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;
using Comax.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComicsController : ControllerBase
    {
        private readonly IComicService _comicService;
        private readonly IChapterService _chapterService;

        public ComicsController(IComicService comicService, IChapterService chapterService)
        {
            _comicService = comicService;
            _chapterService = chapterService;
        }


        [HttpGet]
        public async Task<ActionResult<PagedList<ComicDTO>>> GetAll([FromQuery] PaginationParams paginationParam)
        {
            if (paginationParam == null) paginationParam = new PaginationParams();
            var pagedList = await _comicService.GetAllPagedAsync(paginationParam);
            return Ok(pagedList);
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<ComicDTO>> GetBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return BadRequest(ErrorMessages.Comic.SlugRequired);

            var comic = await _comicService.GetBySlugAsync(slug);

            if (comic == null)
                return NotFound(new { message = string.Format(ErrorMessages.Comic.NotFoundBySlug, slug) });

            if (comic.RowVersion != Guid.Empty)
            {
                Response.Headers.Append("ETag", "\"" + comic.RowVersion.ToString() + "\"");
            }

            return Ok(comic);
        }

       
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 0)
        {
            try
            {
                
                var result = await _comicService.SearchComics(q, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ComicDTO>> GetById(int id)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();

            if (comic.RowVersion != Guid.Empty)
            {
                Response.Headers.Append("ETag", "\"" + comic.RowVersion.ToString() + "\"");
            }
            return Ok(comic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ComicDTO>> Create([FromForm] ComicCreateDTO dto)
        {
            var created = await _comicService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ComicDTO>> Update(int id, [FromForm] ComicUpdateDTO dto)
        {
            if (Request.Headers.TryGetValue("If-Match", out var ifMatch))
            {
                var clientVersionString = ifMatch.ToString().Replace("\"", "");
                var currentEntity = await _comicService.GetByIdAsync(id);

                if (currentEntity == null) return NotFound();

                if (currentEntity.RowVersion.ToString() != clientVersionString)
                {
                    return StatusCode(412, ErrorMessages.System.ConcurrencyConflict);
                }
            }

            try
            {
                var updated = await _comicService.UpdateAsync(id, dto);

                if (updated.RowVersion != Guid.Empty)
                    Response.Headers.Append("ETag", "\"" + updated.RowVersion.ToString() + "\"");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _comicService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "Admin, VipUser")]
        [HttpGet("premium-content")]
        public IActionResult GetPremiumContent()
        {
            return Ok(new { message = ErrorMessages.Auth.PremiumContent });
        }

        [HttpGet("{id}/chapters")]
        public async Task<ActionResult<IEnumerable<ChapterDTO>>> GetChapters(int id)
        {
            var chapters = await _chapterService.GetByComicIdAsync(id);
            return Ok(chapters);
        }

        // --- TÍNH NĂNG THÙNG RÁC ---

        [HttpGet("trash")]
        [Authorize(Roles = "Admin,Mod")]
        public async Task<IActionResult> GetTrash([FromQuery] PaginationParams param, [FromQuery] string? search)
        {
            var result = await _comicService.GetTrashAsync(param, search);

            var metadata = new
            {
                result.TotalCount,
                result.PageSize,
                result.CurrentPage,
                result.TotalPages,
                HasNext = result.CurrentPage < result.TotalPages,
                HasPrevious = result.CurrentPage > 1
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

            return Ok(result);
        }

        [HttpPut("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _comicService.RestoreAsync(id);
            if (!result)
            {
                return NotFound(new { message = SystemMessages.Comic.NotFoundInTrash });
            }
            return Ok(new { message = SystemMessages.Comic.RestoreSuccess });
        }

        [HttpDelete("{id}/purge")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Purge(int id)
        {
            var result = await _comicService.PurgeAsync(id);
            if (!result)
            {
                return NotFound(new { message = SystemMessages.Comic.NotFound });
            }
            return Ok(new { message = SystemMessages.Comic.PurgeSuccess });
        }
        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelated(int id)
        {
            var result = await _comicService.GetRelatedAsync(id);
            return Ok(result);
        }

    }
}