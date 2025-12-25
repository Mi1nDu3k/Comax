using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Common.DTOs.Report;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            var response = new DashboardReportDTO();

            // 1. Số liệu tổng quan
            response.TotalUsers = await _context.Users.CountAsync();
            response.TotalComics = await _context.Comics.CountAsync();
            response.TotalChapters = await _context.Chapters.CountAsync();
            // response.TotalComments = await _context.Comments.CountAsync(); 

            // 2. LOGIC BIỂU ĐỒ USER (7 ngày qua)
            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            // Lấy dữ liệu group theo ngày từ DB
            var userStats = await _context.Users
                .Where(u => u.CreatedAt >= sevenDaysAgo)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Fill dữ liệu vào list (Xử lý trường hợp ngày không có user nào = 0)
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i).Date;
                var stat = userStats.FirstOrDefault(u => u.Date == date);

                response.Labels.Add(date.ToString("dd/MM")); // Nhãn ngày
                response.UserGrowthData.Add(stat?.Count ?? 0); // Số lượng
            }

            // 3. LOGIC BIỂU ĐỒ CATEGORY (Top 5 thể loại)
            var catStats = await _context.Categories
                .Select(c => new
                {
                    Name = c.Name,
                    Count = c.ComicCategories.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            response.CategoryLabels = catStats.Select(x => x.Name).ToList();
            response.ComicByCategoryData = catStats.Select(x => x.Count).ToList();

            return response;
        }

        public async Task<List<TopComicDTO>> GetTopViewedComicsAsync(int top)
        {
            // BƯỚC 1: Lấy dữ liệu thô từ DB về trước (Tránh lỗi dịch SQL int.Parse)
            var comics = await _context.Comics
               .OrderByDescending(c => c.ViewCount)
               .Take(top)
               .ToListAsync();

            // BƯỚC 2: Map sang DTO trong bộ nhớ (In-Memory)
            return comics.Select(c => new TopComicDTO
            {
                Id = c.Id,
                Title = c.Title,
                ThumbnailUrl = c.CoverImage,
                ViewCount = c.ViewCount,
                Rating = c.Rating,
                // Parse an toàn: dùng TryParse để tránh crash nếu Status là null hoặc chuỗi rác
                Status = int.TryParse(c.Status, out int s) ? s : 0
            }).ToList();
        }

        public async Task<List<TopComicDTO>> GetTopRatedComicsAsync(int top)
        {
            // BƯỚC 1: Lấy dữ liệu thô
            var comics = await _context.Comics
               .OrderByDescending(c => c.Rating)
               .Take(top)
               .ToListAsync();

            // BƯỚC 2: Map sang DTO
            return comics.Select(c => new TopComicDTO
            {
                Id = c.Id,
                Title = c.Title,
                ThumbnailUrl = c.CoverImage,
                ViewCount = c.ViewCount,
                Rating = c.Rating,
                // Parse an toàn
                Status = int.TryParse(c.Status, out int s) ? s : 0
            }).ToList();
        }
    }
}