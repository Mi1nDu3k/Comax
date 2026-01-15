    using Comax.Business.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs.Comment;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Comax.API.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        public CommentsController(ICommentService service) { _service = service; }

        [HttpGet("comic/{comicId}")]
        public async Task<IActionResult> GetParents(int comicId, [FromQuery] int page = 1)
        {
            var comments = await _service.GetParentsByComicAsync(comicId, page);
            return Ok(comments);
        }

        [HttpGet("{parentId}/replies")]
        public async Task<IActionResult> GetReplies(int parentId, [FromQuery] int page = 1)
        {
            var replies = await _service.GetRepliesAsync(parentId, page);
            return Ok(replies);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentCreateDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                dto.UserId = int.Parse(userIdClaim.Value);
            }
            else
            {
                Unauthorized(SystemMessages.Comment.LoginRequired);
            }
            var comment = await _service.CreateAsync(dto);
            return Ok(comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommentUpdateDTO dto)
        {
            var comment = await _service.UpdateAsync(id, dto);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            var result = await _service.DeleteAsync(id, hardDelete);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}