using Comax.Business.Interfaces;
using Comax.Common.DTOs.Comment;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
  
    private readonly ICommentService _service;
    public CommentController(ICommentService service) { _service = service; }

    [HttpGet("{comicId}")]
    public async Task<IActionResult> GetByComic(int comicId)
    {
        var comments = await _service.GetByComicAsync(comicId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentCreateDTO dto)
    {
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