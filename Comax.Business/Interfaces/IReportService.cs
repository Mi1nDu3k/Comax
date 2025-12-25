using Comax.Common.DTOs.Report;

namespace Comax.Business.Interfaces
{
    public interface IReportService
    {
        Task<DashboardReportDTO> GetDashboardStatsAsync();
        Task<List<TopComicDTO>> GetTopComicsAsync(string type, int top = 5); // type: "view" hoặc "rating"
    }
}