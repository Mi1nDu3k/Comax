using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly IRatingService _service;
    public CommentController(IRatingService service) { _service = service; }

    [HttpGet("{comicId}")]
    public async Task<IActionResult> GetByComic(int comicId)
    {
        var ratings = await _service.GetByComicAsync(comicId);
        return Ok(ratings);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RatingCreateDTO dto)
    {
        var rating = await _service.CreateAsync(dto);
        return Ok(rating);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RatingUpdateDTO dto)
    {
        var rating = await _service.UpdateAsync(id, dto);
        if (rating == null) return NotFound();
        return Ok(rating);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
