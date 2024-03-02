using System.Text.Json.Serialization;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.CollectionAggregate
{
    public class ReplyComment : Entity
    {
        public string Text { get; set; } = null!;
        public int UserId { get; set; }
        public int CommentId { get; set; }

        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public virtual Comment? Comment { get; set; } = null!;

        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
