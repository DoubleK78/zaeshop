namespace Common.Shared.Models.Users
{
    public class SyncRolesPortalMessage
    {
        public string IdentityUserId { get; set; } = null!;
        public List<string>? Roles { get; set; }
    }
}
