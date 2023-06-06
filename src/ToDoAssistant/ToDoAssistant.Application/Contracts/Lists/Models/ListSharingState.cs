namespace ToDoAssistant.Application.Contracts.Lists.Models;

public enum ListSharingState
{
    NotShared,
    PendingShare,
    Owner,
    Admin,
    Member
}
