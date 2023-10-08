using System.ComponentModel.DataAnnotations;

namespace ToDoAssistant.Api.Models.Lists.Requests;

public record ShareListRequest([Required] int ListId, [Required] List<ShareUserAndPermission> NewShares, [Required] List<ShareUserAndPermission> EditedShares, [Required] List<ShareUserAndPermission> RemovedShares);

public record ShareUserAndPermission([Required] int UserId, [Required] bool IsAdmin);
