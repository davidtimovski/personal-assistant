using Application.Domain.ToDoAssistant;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sentry;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Notifications.Models;

namespace ToDoAssistant.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationsRepository notificationsRepository,
        IMapper mapper,
        ILogger<NotificationService> logger)
    {
        _notificationsRepository = notificationsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<NotificationDto> GetAllAndFlagUnseen(int userId)
    {
        try
        {
            IEnumerable<Notification> notifications = _notificationsRepository.GetAllAndFlagUnseen(userId);

            var result = notifications.Select(x => _mapper.Map<NotificationDto>(x));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAllAndFlagUnseen)}");
            throw;
        }
    }

    public int GetUnseenNotificationsCount(int userId)
    {
        try
        {
            return _notificationsRepository.GetUnseenNotificationsCount(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUnseenNotificationsCount)}");
            throw;
        }
    }

    public Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(NotificationService)}.{nameof(CreateOrUpdateAsync)}");

        try
        {
            var notification = _mapper.Map<Notification>(model);

            notification.CreatedDate = notification.ModifiedDate = DateTime.UtcNow;

            return _notificationsRepository.CreateOrUpdateAsync(notification, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateOrUpdateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }
}
