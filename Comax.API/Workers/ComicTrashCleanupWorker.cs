using Comax.Data; // Thay bằng namespace chứa DbContext của bạn
using Microsoft.EntityFrameworkCore;

namespace Comax.API.Workers
{
    public class ComicTrashCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ComicTrashCleanupWorker> _logger;
        private const int DAYS_TO_KEEP = 3; // Cấu hình: 3 ngày

        public ComicTrashCleanupWorker(IServiceProvider serviceProvider, ILogger<ComicTrashCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(" Comic Trash Cleanup Worker starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Tạo scope mới vì BackgroundService là Singleton, còn DbContext là Scoped
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ComaxDbContext>();

                        var thresholdDate = DateTime.UtcNow.AddDays(-DAYS_TO_KEEP);
                        var deletedCount = await context.Comics
                            .IgnoreQueryFilters()
                            .Where(c => c.IsDeleted && c.DeletedAt <= thresholdDate)
                            .ExecuteDeleteAsync(stoppingToken);

                        if (deletedCount > 0)
                        {
                            _logger.LogInformation($"🗑️ Đã tự động xóa vĩnh viễn {deletedCount} truyện trong thùng rác (quá {DAYS_TO_KEEP} ngày).");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " Lỗi khi chạy dọn dẹp thùng rác Comic.");
                }

                // Chờ 6 tiếng mới quét lại 1 lần để đỡ tốn tài nguyên
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}