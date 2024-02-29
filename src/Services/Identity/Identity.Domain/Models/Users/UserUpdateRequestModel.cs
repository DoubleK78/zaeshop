using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Models.Users
{
    public class UserUpdateRequestModel
    {
        [Required(ErrorMessage = "USER_FULLNAME_REQUIRED")]
        public string FullName { get; set; } = null!;
        
        public string? Password { get; set; }

        public List<string>? Roles { get; set; }
        public bool IsBanned { get; set; }
    }
}