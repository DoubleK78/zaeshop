using System.Text.Json.Serialization;
using Common.Enums;

namespace Portal.Domain.Models.LevelModels
{
    public class LevelBuildRedisModel
    {
        public int UserId { get; set; }

        public int? AlbumId { get; set; }
        public int? CollectionId { get; set; }
        public int? CommentId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string? AdditionalInformation { get; set; }

        public string? IpAddress { get; set; }
        public string? SessionId { get; set; }

        public bool IsViewedNewChapter { get; set; }
    }

    public class LevelBuildRedisRequestModel
    {
        public string IdentityUserId { get; set; } = null!;

        public int? AlbumId { get; set; }
        public int? CollectionId { get; set; }
        public int? CommentId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string? AdditionalInformation { get; set; }

        public string? IpAddress { get; set; }
        public string? SessionId { get; set; }

        public bool IsViewedNewChapter { get; set; }
    }

    public class LevelAdditionalInformation
    {
        public ERoleType RoleType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? AlbumId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CollectionId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CommentId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IpAddress { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SessionId { get; set; }

        public bool IsViewedNewChapter { get; set; }
    }
}
