using Comax.Common.DTOs.Report;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<DashboardReportDTO> GetDashboardStatsAsync();
        Task<List<TopComicDTO>> GetTopViewedComicsAsync(int top);
        Task<List<TopComicDTO>> GetTopRatedComicsAsync(int top);
    }
}