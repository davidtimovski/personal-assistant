﻿using Application.Domain.ToDoAssistant;
using Sentry;

namespace ToDoAssistant.Application.Contracts.Notifications;

public interface INotificationsRepository
{
    IEnumerable<Notification> GetAllAndFlagUnseen(int userId);
    int GetUnseenNotificationsCount(int userId);
    Task DeleteForUserAndListAsync(int userId, int listId, ISpan metricsSpan);
    Task<int> CreateOrUpdateAsync(Notification notification, ISpan metricsSpan);
}
