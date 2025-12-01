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


        // PUBLIC APIs (Dành cho người đọc - Frontend Next.js)

        // GET: api/Comic?PageNumber=1&PageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedList<ComicDTO>>> GetAll([FromQuery] PaginationParams paginationParam)
        {
            if (paginationParam == null) paginationParam = new PaginationParams();
            var pagedList = await _comicService.GetAllPagedAsync(paginationParam);
            return Ok(pagedList);
        }

        // GET: api/Comic/slug/solo-leveling (QUAN TRỌNG CHO SEO)
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<ComicDTO>> GetBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return BadRequest("Slug không được để trống");

            // Gọi service tìm theo slug
            var comic = await _comicService.GetBySlugAsync(slug);

            if (comic == null)
                return NotFound(new { message = $"Không tìm thấy truyện với slug: {slug}" });

            // Trả về ETag để hỗ trợ cache hoặc update sau này
            if (comic.RowVersion != Guid.Empty)
            {
                Response.Headers.Append("ETag", "\"" + comic.RowVersion.ToString() + "\"");
            }

            return Ok(comic);
        }

        // GET: api/Comic/search?title=One
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ComicDTO>>> Search([FromQuery] string title)
        {
            var results = await _comicService.SearchByTitleAsync(title);
            return Ok(results);
        }

        // POST: api/Comic/10/view (Tăng lượt xem)
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncreaseView(int id)
        {
            await _comicService.IncreaseViewCountAsync(id);
            return Ok(new { message = "View count increased" });
        }

 
        // 2. ADMIN APIs (Dành cho trang quản trị - Cần Token)


        // GET: api/Comic/10 (Lấy theo ID để Admin sửa)
        [HttpGet("{id}")]
        public async Task<ActionResult<ComicDTO>> GetById(int id)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();

            // Gắn ETag để xử lý concurrency khi update
            if (comic.RowVersion != Guid.Empty)
            {
                Response.Headers.Append("ETag", "\"" + comic.RowVersion.ToString() + "\"");
            }
            return Ok(comic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ComicDTO>> Create([FromBody] ComicCreateDTO dto)
        {
            // Service đã lo việc tạo Slug tự động
            var created = await _comicService.CreateAsync(dto);

            // Trả về đường dẫn lấy chi tiết theo ID
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ComicDTO>> Update(int id, [FromBody] ComicUpdateDTO dto)
        {
            // Kiểm tra Optimistic Concurrency (ETag)
            // Header If-Match giúp đảm bảo admin không ghi đè dữ liệu cũ nếu có người khác vừa sửa
            if (Request.Headers.TryGetValue("If-Match", out var ifMatch))
            {
                var clientVersionString = ifMatch.ToString().Replace("\"", "");
                var currentEntity = await _comicService.GetByIdAsync(id);

                if (currentEntity == null) return NotFound();

                // Convert RowVersion từ byte[] sang Base64 string để so sánh
                if (currentEntity.RowVersion.ToString() != clientVersionString)
                {
                    return StatusCode(412, "Precondition Failed: Data has been modified by another user.");
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

        // API Test quyền VIP (Giữ lại nếu cần)
        [Authorize(Roles = "Admin, VipUser")]
        [HttpGet("premium-content")]
        public IActionResult GetPremiumContent()
        {
            return Ok(new { message = "Đây là nội dung độc quyền cho VIP!" });
        }
    }
}