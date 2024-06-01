using System.ComponentModel.DataAnnotations;

namespace PersonalAssistant.Web.ViewModels.Friendships;

public record DeclineFriendshipRequest([Required] int FriendId);
