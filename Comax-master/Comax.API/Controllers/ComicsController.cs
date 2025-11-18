using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Comax.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComicsController : ControllerBase
    {
        private readonly IComicService _comicService;
        private readonly IMapper _mapper;

        public ComicsController(IComicService comicService, IMapper mapper)
        {
            _comicService = comicService;
            _mapper = mapper;
        }

        // GET: /api/comics
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comics = await _comicService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ComicDTO>>(comics));
        }

        // GET: /api/comics/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();
            return Ok(_mapper.Map<ComicDTO>(comic));
        }

        // GET: /api/comics/search?title=xxx
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string title)
        {
            var comics = await _comicService.SearchByTitleAsync(title);
            return Ok(_mapper.Map<IEnumerable<ComicDTO>>(comics));
        }

        // GET: /api/comics/author/{authorId}
        [HttpGet("author/{authorId}")]
        public async Task<IActionResult> GetByAuthor(int authorId)
        {
            var comics = await _comicService.GetByAuthorIdAsync(authorId);
            return Ok(_mapper.Map<IEnumerable<ComicDTO>>(comics));
        }

        // GET: /api/comics/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var comics = await _comicService.GetByCategoryIdAsync(categoryId);
            return Ok(_mapper.Map<IEnumerable<ComicDTO>>(comics));
        }

        // POST: /api/comics
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ComicCreateDTO dto)
        {
            var comic = _mapper.Map<Comic>(dto);
            await _comicService.AddAsync(comic, dto.CategoryIds);
            var comicDto = _mapper.Map<ComicDTO>(comic);
            return CreatedAtAction(nameof(GetById), new { id = comic.Id }, comicDto);
        }

        // PUT: /api/comics/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ComicUpdateDTO dto)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();

            _mapper.Map(dto, comic); // Map update fields
            await _comicService.UpdateAsync(comic, dto.CategoryIds);

            return NoContent();
        }

        // DELETE: /api/comics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comic = await _comicService.GetByIdAsync(id);
            if (comic == null) return NotFound();

            await _comicService.DeleteAsync(comic);
            return NoContent();
        }
    }
}
