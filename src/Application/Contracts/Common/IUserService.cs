﻿using System.Threading.Tasks;
using Application.Contracts.Common.Models;
using Domain.Entities.Common;

namespace Application.Contracts.Common;

public interface IUserService
{
    User Get(int id);
    User Get(string email);
    T Get<T>(int id) where T : UserDto;
    bool Exists(int id);
    CookingAssistantPreferences GetCookingAssistantPreferences(int id);
    Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri);
    Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled);
    Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled);
    Task UpdateImperialSystemAsync(int id, bool imperialSystem);
}
