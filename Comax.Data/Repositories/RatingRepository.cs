using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class RatingRepository : BaseRepository<Rating>, IRatingRepository
    {
        public RatingRepository(ComaxDbContext context) : base(context) { }

        // --- CÁC HÀM CŨ ---
        public async Task<List<Rating>> GetByComicAsync(int comicId)
        {
            return await _dbSet
                .Where(r => r.ComicId == comicId)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<double> GetAverageScoreAsync(int comicId)
        {
            var ratings = _dbSet.Where(r => r.ComicId == comicId);
            if (!await ratings.AnyAsync()) return 0;
            return await ratings.AverageAsync(r => r.Score);
        }

        // --- BỔ SUNG HÀM NÀY ĐỂ SỬA LỖI ---
        public async Task<Rating?> GetByUserAndComicAsync(int userId, int comicId)
        {
            // Tìm đánh giá khớp cả UserID và ComicID
            return await _dbSet.FirstOrDefaultAsync(r => r.UserId == userId && r.ComicId == comicId);
        }
    }
}