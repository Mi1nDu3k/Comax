using Comax.API.Hubs;
using Comax.Business.Services.Interfaces;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private const int BATCH_SIZE = 100;

        public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int page, int pageSize)
        {
            return await _unitOfWork.Notifications.GetByUserIdAsync(userId, page, pageSize);
        }

        public async Task CreateAndSendNotificationAsync(int userId, string message, string url)
        {
            var noti = new Notification
            {
                UserId = userId,

                Message = message,
                Url = url,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(noti);
            await _unitOfWork.CommitAsync();

            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            {
                id = noti.Id,
                message = noti.Message,
                url = noti.Url,
                isRead = false,
                createdAt = noti.CreatedAt
            });
        }

        public async Task SendNotificationToGroupAsync(List<int> userIds, string message, string url)
        {
            if (userIds == null || !userIds.Any()) return;

            var batches = userIds.Chunk(BATCH_SIZE);
            var now = DateTime.UtcNow;

            foreach (var batch in batches)
            {
                var notiList = new List<Notification>();
                foreach (var uid in batch)
                {
                    notiList.Add(new Notification
                    {
                        UserId = uid,
                        Message = message,
                        Url = url,
                        IsRead = false,
                        CreatedAt = now
                    });
                }

                await _unitOfWork.Notifications.AddRangeAsync(notiList);
                await _unitOfWork.CommitAsync();

                var tasks = new List<Task>();
                foreach (var noti in notiList)
                {
                    tasks.Add(_hubContext.Clients.Group($"User_{noti.UserId}")
                        .SendAsync("ReceiveNotification", new
                        {
                            id = noti.Id,
                            message = noti.Message,
                            url = noti.Url,
                            isRead = false,
                            createdAt = noti.CreatedAt
                        }));
                }
                await Task.WhenAll(tasks);
                await Task.Delay(20);
            }
        }

        public async Task MarkAsReadAsync(int id)
        {
            var noti = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (noti != null)
            {
                noti.IsRead = true;
                _unitOfWork.Notifications.Update(noti);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.CommitAsync();
        }

        // --- SỬA TẠI ĐÂY ---
        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Notifications.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }
    }
}