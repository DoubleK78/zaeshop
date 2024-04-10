using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Models.Users;

public class UserFingerPrintModel
{
    [Required]
    public string FingerPrint { get; set; } = null!;

    [StringLength(350)]
    public string? AdditionalDetail { get; set; }
}
