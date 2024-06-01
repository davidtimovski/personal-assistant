using System.ComponentModel.DataAnnotations;

namespace PersonalAssistant.Web.ViewModels.Friendships;

public record AcceptFriendshipRequest([Required] int FriendId);
