using System.Text.Json.Serialization;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.CollectionAggregate
{
    public class Comment : Entity
    {
        public string Text { get; set; } = null!;

        public int AlbumId { get; set; }
        public int? CollectionId { get; set; }
        public int UserId { get; set; }

        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual Album? Album { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<ReplyComment>? ReplyComments { get; set; } = new List<ReplyComment>();

        [JsonIgnore]
        public virtual Collection? Collection { get; set; } = null!;

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
