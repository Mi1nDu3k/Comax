using Comax.Data.Repositories.Interfaces;
using Comax.Common.DTOs.Report;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ComaxDbContext _context;

        public ReportRepository(ComaxDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardReportDTO> GetDashboardStatsAsync()
        {
            return new DashboardReportDTO
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalComics = await _context.Comics.CountAsync(),
                TotalChapters = await _context.Chapters.CountAsync(),
                TotalComments = await _context.Comments.CountAsync()
            };
        }

        public async Task<List<TopComicDTO>> GetTopViewedComicsAsync(int top)
        {
            return await _context.Comics
                .OrderByDescending(c => c.ViewCount)
                .Take(top)
                .Select(c => new TopComicDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    ViewCount = c.ViewCount,
                    AverageRating = c.Rating, 
                    RatingCount = _context.Ratings.Count(r => r.ComicId == c.Id)
                })
                .ToListAsync();
        }

        public async Task<List<TopComicDTO>> GetTopRatedComicsAsync(int top)
        {
            var topStats = await _context.Ratings
                .GroupBy(r => r.ComicId)
                .Select(g => new
                {
                    ComicId = g.Key,
                    AverageRating = g.Average(r => r.Score),
                    RatingCount = g.Count()
                })
                .OrderByDescending(x => x.AverageRating) 
                .Take(top) 
                .ToListAsync();

            if (!topStats.Any()) return new List<TopComicDTO>();

           
            var comicIds = topStats.Select(s => s.ComicId).ToList();

            
            var comics = await _context.Comics
                .Where(c => comicIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.ViewCount
                  
                })
                .ToListAsync();

           
            var result = topStats.Join(comics,
                stat => stat.ComicId,
                comic => comic.Id,
                (stat, comic) => new TopComicDTO
                {
                    Id = comic.Id,
                    Title = comic.Title,
                    ViewCount = comic.ViewCount,
                    AverageRating = Math.Round(stat.AverageRating, 1), 
                    RatingCount = stat.RatingCount
                })
                .OrderByDescending(x => x.AverageRating) 
                .ToList();

            return result;
        }
    }
}