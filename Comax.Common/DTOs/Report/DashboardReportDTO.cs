namespace Comax.Common.DTOs.Report
{
    public class DashboardReportDTO
    {
        public int TotalUsers { get; set; }
        public int TotalComics { get; set; }
        public int TotalChapters { get; set; }
        public int TotalComments { get; set; }

        // Dữ liệu biểu đồ
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> UserGrowthData { get; set; } = new List<int>();
        public List<string> CategoryLabels { get; set; } = new List<string>();
        public List<int> ComicByCategoryData { get; set; } = new List<int>();
    }
}