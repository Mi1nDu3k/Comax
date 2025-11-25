
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Author;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _service;

    public AuthorController(IAuthorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var author = await _service.GetByIdAsync(id);
        if (author == null) return NotFound();
        return Ok(author);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AuthorCreateDTO dto)
    {
        var author = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO dto)
    {
        var author = await _service.UpdateAsync(id, dto);
        if (author == null) return NotFound();
        return Ok(author);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
    {
        var result = await _service.DeleteAsync(id, hardDelete);
        if (!result) return NotFound();
        return NoContent();
    }
}