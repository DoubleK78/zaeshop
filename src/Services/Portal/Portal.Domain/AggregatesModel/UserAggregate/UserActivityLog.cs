using Portal.Domain.Enums;
using Portal.Domain.SeedWork;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Portal.Domain.AggregatesModel.UserAggregate
{
    public class UserActivityLog : Entity
    {
        public string? Description { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? IpV4Address { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? IpV6Address { get; set; }
        
        public EActivityType ActivityType { get; set; }
        public int LogTimes { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
