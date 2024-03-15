using System.Text.Json.Serialization;
using Common.Enums;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.Enums;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.UserAggregate;
public class User : Entity, IAggregateRoot
{
    public string FullName { get; set; } = null!;
    public string IdentityUserId { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;

    public string? Avatar { get; set; }
    public ERoleType RoleType { get; set; }

    public int? LevelId { get; set; }
    public int CurrentExp { get; set; }
    public int NextLevelExp { get; set; }
    public DateTime? ExpriedRoleDate { get; set; }
    public ERegion Region { get; set; }
    public bool IsBanned { get; set; }

    public ERemindSubscription RemindSubscription { get; set; }

    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [JsonIgnore]
    public virtual ICollection<CollectionView> CollectionViews { get; set; } = new List<CollectionView>();

    [JsonIgnore]
    public virtual ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();

    [JsonIgnore]
    public virtual ICollection<UserActivityLog> UserActivityLogs { get; set; } = new List<UserActivityLog>();

    [JsonIgnore]
    public virtual ICollection<Following> Followings { get; set; } = new List<Following>();

    [JsonIgnore]
    public virtual Level? Level { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserLevel> UserLevels { get; set; } = new List<UserLevel>();

    [JsonIgnore]
    public virtual ICollection<ReplyComment>? ReplyComments { get; set; } = new List<ReplyComment>();

    [JsonIgnore]
    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}
