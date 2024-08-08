using System.ComponentModel.DataAnnotations.Schema;
using Identity.Domain.SeedWork;

namespace Identity.Domain.AggregatesModel.UserAggregate;

public class UserActivityLog : Entity, IAggregateRoot
{
    public string? UserId { get; set; }
    public DateTime EventDate { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? IpAddress { get; set; }

    [Column(TypeName = "varchar(1000)")]
    public string? AdditionalDetail { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? Fingerprinting { get; set; }

    public int LogTimes { get; set; }

    public bool IsExceed => LogTimes > 5;
}
