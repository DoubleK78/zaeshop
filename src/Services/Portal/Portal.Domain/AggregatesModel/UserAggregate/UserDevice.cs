using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enums;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.UserAggregate
{
    public class UserDevice : Entity
    {
        /// <summary>
        /// Firebase Cloud Messaging will be created token to receive push notification
        /// </summary>
        [Column(TypeName = "varchar(500)")]
        public string RegistrationToken { get; set; } = null!;

        public EDeviceType DeviceType { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? BrowserVersion { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? ScreenResolution { get; set; }

        public bool IsEnabled { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
