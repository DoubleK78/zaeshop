using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.UserAggregate
{
    public class UserLevel : Entity
    {
        public int LevelId { get; set; }
        public int UserId { get; set; }
        public int CurrentExp { get; set; }

        public string? AdditionalInformation { get; set; }

        public DateTime Date { get; set; }
        public int Exp { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? IpAddress { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? SessionId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [JsonIgnore]
        public virtual Level Level { get; set; } = null!;
    }
}
