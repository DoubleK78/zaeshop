using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.Enums;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.AlbumAggregate
{
    public class Album : Entity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? AlbumAlertMessageId { get; set; }

        #region More Infomation
        public string? AlternativeName { get; set; }
        public string? Type { get; set; }
        public EAlbumStatus AlbumStatus { get; set; }
        public string? ReleaseYear { get; set; }
        public string? AuthorNames { get; set; }
        public string? ArtitstNames { get; set; }
        public string? Tags { get; set; }
        public string? OriginalUrl { get; set; }
        public string? CdnOriginalUrl { get; set; }
        #endregion

        public bool IsPublic { get; set; }
        public ELevelPublic LevelPublic { get; set; }
        public ERegion Region { get; set; }

        [Column(TypeName = "varchar(350)")]
        public string? FriendlyName { get; set; }

        #region Thumbnail
        public string? ThumbnailUrl { get; set; }
        public string? CdnThumbnailUrl { get; set; }
        #endregion

        public ulong Views { get; set; }
        public ulong ViewsByTopDay { get; set; }
        public ulong ViewsByTopWeek { get; set; }
        public ulong ViewsByTopMonth { get; set; }
        public ulong ViewsByTopYear { get; set; }

        [JsonIgnore]
        public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

        [JsonIgnore]
        public virtual ICollection<Following> Followers { get; set; } = new List<Following>();

        [JsonIgnore]
        public virtual ICollection<AlbumContentType> AlbumContentTypes { get; set; } = new List<AlbumContentType>();

        [JsonIgnore]
        public virtual AlbumAlertMessage? AlbumAlertMessage { get; set; }

        [JsonIgnore]
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
