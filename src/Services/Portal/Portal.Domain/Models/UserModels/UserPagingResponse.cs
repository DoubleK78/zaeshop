using Common.Enums;
using Portal.Domain.Enums;
using System.Text.Json.Serialization;

namespace Portal.Domain.Models.UserModels
{
    public class UserPagingResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? Avatar { get; set; }
        public ERoleType RoleType { get; set; }
        public int? LevelId { get; set; }
        public int CurrentExp { get; set; }
        public int NextLevelExp { get; set; }
        public ERegion Region { get; set; }
        [JsonIgnore]
        public long RowNum { get; set; }

        [JsonIgnore]
        public bool IsTotalRecord { get; set; }
    }
}
