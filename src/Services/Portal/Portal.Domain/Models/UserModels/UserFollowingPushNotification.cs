using Common.Enums;

namespace Portal.Domain.Models.UserModels
{
    public class UserFollowingPushNotification
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }

        public ERoleType RoleType { get; set; }
        public List<string> RegistrationTokens { get; set; } = new List<string>();
    }
}
