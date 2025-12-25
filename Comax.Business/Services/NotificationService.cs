using AutoMapper;
using Comax.Business.Hubs;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Notification;
using Comax.Common.Enums;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(int userId)
        {
            var entities = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            return _mapper.Map<List<NotificationDTO>>(entities);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var noti = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (noti != null && !noti.IsRead)
            {
                noti.IsRead = true;
                // Sửa lỗi: Dùng UpdateAsync thay vì Update
                await _unitOfWork.Notifications.UpdateAsync(noti);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteAsync(int notificationId, int userId)
        {
            var noti = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (noti != null && noti.UserId == userId)
            {
                // Sửa lỗi: Dùng DeleteAsync có sẵn trong BaseRepository
                await _unitOfWork.Notifications.DeleteAsync(notificationId);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task CreateAndSendNotificationAsync(int userId, string message, string url, NotificationType type = NotificationType.System)
        {
            var noti = new Notification
            {
                UserId = userId,
                Message = message,
                Url = url,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(noti);
            await _unitOfWork.CommitAsync();
            var dto = _mapper.Map<NotificationDTO>(noti);
            if (_hubContext != null)
            {
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", dto);
            }
        }
    }
}