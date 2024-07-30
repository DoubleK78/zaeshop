using Common.Enums;

namespace Portal.Domain.Models.UserModels
{
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Avatar { get; set; }

        public ERoleType RoleType { get; set; }
        public int? LevelId { get; set; }
        public int CurrentExp { get; set; }
        public int NextLevelExp { get; set; }
        public DateTime? ExpriedRoleDate { get; set; }
    }
}
