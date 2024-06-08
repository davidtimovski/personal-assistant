namespace ToDoAssistant.Api.Models.Lists.Requests;

public record ShareListRequest(int ListId, List<ShareUserAndPermission> NewShares, List<ShareUserAndPermission> EditedShares, List<ShareUserAndPermission> RemovedShares);

public record ShareUserAndPermission(int UserId, bool IsAdmin);
