using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Notification;
using Comax.Common.Enums;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<NotificationDTO>> GetMyNotificationsAsync(int userId)
        {
            
            var entities = await _unitOfWork.Notifications.GetByUserAsync(userId);
            return _mapper.Map<List<NotificationDTO>>(entities);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var noti = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (noti != null && !noti.IsRead)
            {
                noti.IsRead = true;
                await _unitOfWork.Notifications.UpdateAsync(noti);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            // Hàm này nằm trong Repository để xử lý batch update
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.CommitAsync();
        }

        // --- Hàm bạn vừa viết ---
        public async Task CreateAsync(int userId, string message, string url, NotificationType type)
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
        }
    }
}