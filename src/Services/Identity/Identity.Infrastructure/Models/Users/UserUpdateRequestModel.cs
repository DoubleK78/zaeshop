using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Models.Users
{
    public class UserUpdateRequestModel
    {
        [Required(ErrorMessage = "USER_FULLNAME_REQUIRED")]
        public string FullName { get; set; } = null!;
        
        public string? Password { get; set; }
    }
}