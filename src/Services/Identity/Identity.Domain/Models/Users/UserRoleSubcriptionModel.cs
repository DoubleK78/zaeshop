using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Models.Users
{
    public class UserRoleSubcriptionModel
    {
        public string UserId { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }

        public string? Role { get; set; }
        public DateTime? ExpriedRoleDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
    }

    public class UserRoleSubcriptionRequestModel
    {
        public string UserId { get; set; } = null!;

        public string Role { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "error_user_role_subcription_days_invalid")]
        public int? Days { get; set; }
    }

    public class UserRoleNewSubscription
    {
        public List<string> Role { get; set; } = new List<string>();
        public DateTime? ExpriedRoleDate { get; set; }
    }
}
