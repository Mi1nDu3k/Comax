using Comax.Business.Services;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;
using Comax.Common.Helpers;
using Comax.Shared; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// GET: api/Comic
        /// </summary>
        /// <param name="paginationParam"></param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<ActionResult<PagedList<ComicDTO>>> GetAll([FromQuery] PaginationParams paginationParam)
        //{
        //    if (paginationParam == null) paginationParam = new PaginationParams();
        //    var pagedList = await _comicService.GetAllPagedAsync(paginationParam);
        //    return Ok(pagedList);
        //}
        [HttpGet]
     
        public async Task<IActionResult> GetComics([FromQuery] PaginationParams paginationParams)
        {
            var result = await _comicService.GetAllPagedAsync(paginationParams);

            
            return Ok(result);
        }
        /// <summary>
        /// GET: api/Comic/slug/{slug}
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
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
        /// <summary>
        /// GET: api/Comic/search
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ComicDTO>>> Search([FromQuery] string title)
        {
            var results = await _comicService.SearchByTitleAsync(title);
            return Ok(results);
        }

        /// <summary>
        /// POST: api/Comic/{id}/view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncreaseView(int id)
        {
            await _comicService.IncreaseViewCountAsync(id);
            return Ok(new { message = ErrorMessages.Comic.ViewCountIncreased }); 
        }

        // --- ADMIN APIs ---

        [HttpGet("{id}")]
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

        // ...
        [Authorize(Roles = "Admin")]
        [HttpPost]
        // Đổi [FromBody] -> [FromForm]
        public async Task<ActionResult<ComicDTO>> Create([FromForm] ComicCreateDTO dto)
        {
            var created = await _comicService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ComicDTO>> Update(int id, [FromForm] ComicUpdateDTO dto)
        {
            /// <summary>
            /// kiểm tra concurrency token từ header If-Match
            /// </summary>
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
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _comicService.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
        /// <summary>
        /// API Test quyền VIP
        /// </summary>
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
    }
}