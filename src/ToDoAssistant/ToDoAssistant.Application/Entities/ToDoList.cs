﻿using Core.Application;
using Core.Application.Entities;

namespace ToDoAssistant.Application.Entities;

public class ToDoList : Entity
{
    public const string DefaultIcon = "Regular";
    public const int MaxTasks = 400;

    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public short? Order { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool IsOneTimeToggleDefault { get; set; }
    public bool IsArchived { get; set; }

    public User? User { get; set; }
    public List<ToDoTask> Tasks { get; set; } = new();
    public List<ListShare> Shares { get; set; } = new();

    public bool IsShared => Shares.Any(x => x.IsAccepted == true);
}
