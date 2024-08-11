using System.ComponentModel.DataAnnotations.Schema;
using Identity.Domain.SeedWork;

namespace Identity.Domain.AggregatesModel.UserAggregate;

public class UserActivityLog : Entity, IAggregateRoot
{
    public DateTime EventDate { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? IpAddress { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? Description { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? BrowserFingerprint { get; set; }

    public string? UserId { get; set; }
}
