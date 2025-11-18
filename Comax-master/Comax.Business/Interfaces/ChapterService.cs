using Comax.Business.Interfaces;
using Comax.Data.Entities;
using Comax.Data.Repositories;

namespace Comax.Business.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _repository;

        public ChapterService(IChapterRepository repository) => _repository = repository;

        public Task<IEnumerable<Chapter>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Chapter?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId) => _repository.GetByComicIdAsync(comicId);
        public Task AddAsync(Chapter chapter) => _repository.AddAsync(chapter);
        public Task UpdateAsync(Chapter chapter) => _repository.UpdateAsync(chapter);
        public Task DeleteAsync(Chapter chapter) => _repository.DeleteAsync(chapter);
    }
}
