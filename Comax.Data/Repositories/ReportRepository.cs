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
 
                    AverageRating = _context.Ratings.Where(r => r.ComicId == c.Id).Average(r => (double?)r.Score) ?? 0,
                    RatingCount = _context.Ratings.Count(r => r.ComicId == c.Id)
                })
                .ToListAsync();
        }

        public async Task<List<TopComicDTO>> GetTopRatedComicsAsync(int top)
        {
          

            var topRatedData = await _context.Ratings
                .GroupBy(r => r.ComicId)
                .Select(g => new
                {
                    ComicId = g.Key,
                    AvgScore = g.Average(r => r.Score),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.AvgScore)
                .Take(top)
                .ToListAsync();

            //  Join lại với bảng Comic để lấy Title
            var result = new List<TopComicDTO>();
            foreach (var item in topRatedData)
            {
                var comic = await _context.Comics.FindAsync(item.ComicId);
                if (comic != null)
                {
                    result.Add(new TopComicDTO
                    {
                        Id = comic.Id,
                        Title = comic.Title,
                        ViewCount = comic.ViewCount,
                        AverageRating = item.AvgScore,
                        RatingCount = item.Count
                    });
                }
            }
            return result;
        }
    }
}