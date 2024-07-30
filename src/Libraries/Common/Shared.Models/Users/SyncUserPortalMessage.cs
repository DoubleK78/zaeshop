namespace Common.Shared.Models.Users
{
    public class SyncUserPortalMessage
    {
        public string IdentityUserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Avatar { get; set; }
        public bool IsUpdateAvatar { get; set; }
        public string? Region { get; set; }
    }
}
