using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Utils;
using Microsoft.Extensions.Logging;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Notifications.Models;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationsRepository? notificationsRepository,
        IMapper? mapper,
        ILogger<NotificationService>? logger)
    {
        _notificationsRepository = ArgValidator.NotNull(notificationsRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public Result<IReadOnlyList<NotificationDto>> GetAllAndFlagUnseen(int userId)
    {
        try
        {
            var notifications = _notificationsRepository.GetAllAndFlagUnseen(userId);

            var result = notifications.Select(x => _mapper.Map<NotificationDto>(x)).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAllAndFlagUnseen)}");
            return new();
        }
    }

    public Result<int> GetUnseenNotificationsCount(int userId)
    {
        try
        {
            var count = _notificationsRepository.GetUnseenNotificationsCount(userId);
            return new(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUnseenNotificationsCount)}");
            return new();
        }
    }

    public async Task<Result<int>> CreateOrUpdateAsync(CreateOrUpdateNotification model, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(NotificationService)}.{nameof(CreateOrUpdateAsync)}");

        try
        {
            var notification = _mapper.Map<Notification>(model);

            notification.CreatedDate = notification.ModifiedDate = DateTime.UtcNow;

            var id = await _notificationsRepository.CreateOrUpdateAsync(notification, metric, cancellationToken);

            return new(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateOrUpdateAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }
}
