using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class RatingRepository : BaseRepository<Rating>, IRatingRepository
    {
        public RatingRepository(ComaxDbContext context) : base(context) { }

        public async Task<List<Rating>> GetByComicAsync(int comicId)
        {
            return await _context.Ratings
                .Where(r => r.ComicId == comicId)
                .ToListAsync();
        }


        public async Task<double> GetAverageScoreAsync(int comicId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.ComicId == comicId)
                .ToListAsync();
            return ratings.Any() ? ratings.Average(r => r.Score) : 0;
        }
    }

    
}