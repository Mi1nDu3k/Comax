using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _service;
    public RatingsController(IRatingService service) { _service = service; }

    [HttpGet("{comicId}")]
    public async Task<IActionResult> GetByComic(int comicId)
    {
        var ratings = await _service.GetByComicAsync(comicId);
        return Ok(ratings);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RatingCreateDTO dto)
    {
        var rating = await _service.CreateAsync(dto);
        return Ok(rating);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RatingUpdateDTO dto)
    {
        var rating = await _service.UpdateAsync(id, dto);
        if (rating == null) return NotFound();
        return Ok(rating);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
    {
        var result = await _service.DeleteAsync(id, hardDelete);
        if (!result) return NotFound();
        return NoContent();
    }
}