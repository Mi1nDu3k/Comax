using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;

public class ChapterService : IChapterService
{
    private readonly IChapterRepository _repo;
    private readonly IMapper _mapper;

    public ChapterService(IChapterRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ChapterDTO> CreateAsync(ChapterCreateDTO dto)
    {
        var entity = _mapper.Map<Chapter>(dto);
        await _repo.AddAsync(entity);
        return _mapper.Map<ChapterDTO>(entity);
    }

    public async Task<ChapterDTO> UpdateAsync(int id, ChapterUpdateDTO dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);
        return _mapper.Map<ChapterDTO>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        await _repo.DeleteAsync(entity);
        return true;
    }

    public async Task<ChapterDTO?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return _mapper.Map<ChapterDTO>(entity);
    }

    public async Task<IEnumerable<ChapterDTO>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<ChapterDTO>>(entities);
    }
}
