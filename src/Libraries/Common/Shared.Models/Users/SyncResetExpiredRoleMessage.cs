namespace Common.Shared.Models.Users
{
    public class SyncResetExpiredRoleMessage
    {
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
