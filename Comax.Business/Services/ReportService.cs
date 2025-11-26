using Comax.Business.Interfaces;
using Comax.Data.Repositories.Interfaces;
using Comax.Common.DTOs.Report;

namespace Comax.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardReportDTO> GetDashboardStatsAsync()
        {
            return await _repo.GetDashboardStatsAsync();
        }

        public async Task<List<TopComicDTO>> GetTopComicsAsync(string type, int top = 5)
        {
            if (type.ToLower() == "view")
            {
                return await _repo.GetTopViewedComicsAsync(top);
            }
            else
            {
                return await _repo.GetTopRatedComicsAsync(top);
            }
        }
    }
}