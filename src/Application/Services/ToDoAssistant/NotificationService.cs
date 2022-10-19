﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Notifications;
using Application.Contracts.ToDoAssistant.Notifications.Models;
using AutoMapper;
using Domain.Entities.ToDoAssistant;
using Microsoft.Extensions.Logging;

namespace Application.Services.ToDoAssistant;

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

    public Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model)
    {
        try
        {
            var notification = _mapper.Map<Notification>(model);

            notification.CreatedDate = notification.ModifiedDate = DateTime.UtcNow;

            return _notificationsRepository.CreateOrUpdateAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateOrUpdateAsync)}");
            throw;
        }
    }
}
