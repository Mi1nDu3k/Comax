using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces; // Đảm bảo đúng namespace
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class RatingRepository : BaseRepository<Rating>, IRatingRepository
    {
        public RatingRepository(ComaxDbContext context) : base(context) { }

        public async Task<List<Rating>> GetByComicAsync(int comicId)
        {
            return await _dbSet
                .Where(r => r.ComicId == comicId)
                .Include(r => r.User) // Để hiển thị tên người vote nếu cần
                .ToListAsync();
        }

        public async Task<double> GetAverageScoreAsync(int comicId)
        {
            var ratings = _dbSet.Where(r => r.ComicId == comicId);
            if (!await ratings.AnyAsync()) return 0;
            return await ratings.AverageAsync(r => r.Score);
        }
        public async Task<Rating?> GetUserRatingAsync(int userId, int comicId)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.UserId == userId && r.ComicId == comicId);
        }
    }
}