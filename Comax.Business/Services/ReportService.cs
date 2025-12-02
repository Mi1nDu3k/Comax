using Comax.Business.Interfaces;
using Comax.Common.DTOs.Report;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory; // Thêm
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        private readonly IMemoryCache _cache; // Inject

        public ReportService(IReportRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<DashboardReportDTO> GetDashboardStatsAsync()
        {
            // Cache Dashboard 5 phút
            return await _cache.GetOrCreateAsync("dashboard_stats", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _repo.GetDashboardStatsAsync();
            });
        }

        public async Task<List<TopComicDTO>> GetTopComicsAsync(string type, int top = 5)
        {
            // Tạo Key cache duy nhất cho từng loại query: "top_view_5", "top_rating_10"...
            string key = $"top_{type}_{top}";

            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                // Cache BXH 10 phút
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                if (type.ToLower() == "view")
                {
                    return await _repo.GetTopViewedComicsAsync(top);
                }
                else
                {
                    return await _repo.GetTopRatedComicsAsync(top);
                }
            });
        }
    }
}