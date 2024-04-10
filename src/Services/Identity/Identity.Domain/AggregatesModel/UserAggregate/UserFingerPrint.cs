using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Identity.Domain.SeedWork;

namespace Identity.Domain.AggregatesModel.UserAggregate;

public class UserFingerPrint : Entity, IAggregateRoot
{
    [Column(TypeName = "varchar(150)")]
    public string FingerPrint { get; set; } = null!;
    public bool IsBanned { get; set; }
    public int? UserId { get; set; }

    [Column(TypeName = "varchar(500)")]
    public string? AdditionalDetail { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}