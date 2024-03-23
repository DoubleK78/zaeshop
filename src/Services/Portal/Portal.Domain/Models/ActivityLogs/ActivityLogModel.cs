
using Portal.Domain.Enums;

namespace Portal.Domain.Models.ActivityLogs
{
    public class ActivityLogRequestModel
    {
        public string? Description { get; set; }
        public string? IpV4Address { get; set; }
        public string? IpV6Address { get; set; }
        public EActivityType? ActivityType { get; set; }
        public int? LogTimes { get; set; }
        public int? UserId { get; set; }
        public int? LimitTimes { get; set; }
    }

    public class ActivityLogResponseModel
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? IpV4Address { get; set; }
        public string? IpV6Address { get; set; }
        public EActivityType ActivityType { get; set; }
        public int LogTimes { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
