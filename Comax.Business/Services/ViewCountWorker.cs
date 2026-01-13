using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Comax.Data;
using Microsoft.EntityFrameworkCore;

namespace Comax.Business.Services
{
    public class ViewCountWorker : BackgroundService
    {
        private readonly IViewCountBuffer _buffer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ViewCountWorker> _logger;
        private readonly PeriodicTimer _timer;

        public ViewCountWorker(
            IViewCountBuffer buffer,
            IServiceProvider serviceProvider,
            ILogger<ViewCountWorker> logger)
        {
            _buffer = buffer;
            _serviceProvider = serviceProvider;
            _logger = logger;
            // Cấu hình thời gian flush: Ví dụ 10 giây 1 lần
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await FlushViewsToDatabaseAsync();
            }
        }

        private async Task FlushViewsToDatabaseAsync()
        {
            // 1. Lấy toàn bộ view đang chờ trong RAM
            var views = _buffer.PopAll();
            if (views.Count == 0) return;

            _logger.LogInformation($"Updating views for {views.Count} comics...");

            // 2. Tạo Scope mới để lấy DbContext (vì Worker là Singleton, DbContext là Scoped)
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ComaxDbContext>();

                foreach (var item in views)
                {
                    int comicId = item.Key;
                    int viewsToAdd = item.Value;

                    // 3. TỐI ƯU HÓA CAO CẤP (.NET 8): ExecuteUpdateAsync
                    // Update trực tiếp trên SQL mà KHÔNG cần Query lấy Entity lên trước.
                    // Giải quyết hoàn toàn vấn đề Concurrency và Nhanh hơn gấp nhiều lần.
                    await context.Comics
                        .Where(c => c.Id == comicId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(c => c.ViewCount, c => c.ViewCount + viewsToAdd)
                            .SetProperty(c => c.RowVersion, Guid.NewGuid()) // Cập nhật RowVersion để báo hiệu data thay đổi
                        );
                }
            }
        }
    }
}