
using System.ComponentModel.DataAnnotations;

namespace PersonalAssistant.Web.ViewModels.Friendships;

public record DeleteFriendshipRequest([Required] int FriendId);
