using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.ToDoAssistant;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Services.ToDoAssistant
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationsRepository notificationsRepository,
            IMapper mapper)
        {
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllAndFlagUnseenAsync(int userId)
        {
            IEnumerable<Notification> notifications = await _notificationsRepository.GetAllAndFlagUnseenAsync(userId);

            var result = notifications.Select(x => _mapper.Map<NotificationDto>(x));

            return result;
        }

        public Task<int> GetUnseenNotificationsCountAsync(int userId)
        {
            return _notificationsRepository.GetUnseenNotificationsCountAsync(userId);
        }

        public Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model)
        {
            var notification = _mapper.Map<Notification>(model);

            notification.CreatedDate = notification.ModifiedDate = DateTime.UtcNow;

            return _notificationsRepository.CreateOrUpdateAsync(notification);
        }
    }
}
