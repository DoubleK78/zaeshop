using Common.Enums;

namespace Common.Shared.Models.Users
{
    public class SyncRolesPortalMessage
    {
        public string IdentityUserId { get; set; } = null!;
        public int? Days { get; set; }
        public List<string>? Roles { get; set; }
        public bool IsBanned { get; set; }

        #region For Update Expried Role Date
        public bool IsUpdateSubscription { get; set; }
        public ERoleType OldRoleType { get; set; }
        public ERoleType NewRoleType { get; set; }
        public DateTime? ExpriedRoleDate { get; set; }
        #endregion
    }
}
