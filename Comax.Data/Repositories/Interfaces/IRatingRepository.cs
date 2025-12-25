using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public interface IRatingRepository : IBaseRepository<Rating>
    {
        Task<List<Rating>> GetByComicAsync(int comicId);
        Task<double> GetAverageScoreAsync(int comicId);
        Task<Rating?> GetUserRatingAsync(int userId, int comicId);
    }
}