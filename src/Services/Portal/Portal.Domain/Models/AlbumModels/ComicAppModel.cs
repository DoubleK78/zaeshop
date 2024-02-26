using Portal.Domain.Enums;
using Portal.Domain.Models.CollectionModels;

namespace Portal.Domain.Models.AlbumModels
{
    public class ComicAppModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? AlbumAlertMessageName { get; set; }
        public string? ContentTypeNames { get; set; }

        #region Comic Extra info
        public string? AlternativeName { get; set; }
        public string? Type { get; set; }
        public EAlbumStatus AlbumStatus { get; set; }
        public string? ReleaseYear { get; set; }
        public string? AuthorNames { get; set; }
        public string? ArtitstNames { get; set; }
        public string? Tags { get; set; }
        #endregion

        #region Thumbnail
        public string? ThumbnailUrl { get; set; }
        public string? CdnThumbnailUrl { get; set; }
        #endregion

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public bool IsPublic { get; set; }
        public string? FriendlyName { get; set; }

        public ulong Views { get; set; }
        public ELevelPublic LevelPublic { get; set; }
        public ERegion Region { get; set; }

        public List<ContentAppModel> Contents { get; set; } = [];
    }

    #region Next SEO
    public class AlbumMetadataModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? FriendlyName { get; set; }
        public string? LastestChapter { get; set; }
        public string? ComicImageUrl { get; set; }
        public ERegion Region { get; set; }
    }

    public class ComicMetadata
    {
        public string Title { get; set; } = null!;
        public string? LastestChapter { get; set; }
        public string? ComicImageUrl { get; set; }
        public ERegion Region { get; set; }
    }

    public class ComicSitemap
    {
        public string? FriendlyName { get; set; }
        public ERegion Region { get; set; }
        public List<string> ContentFriendlyNames { get; set; } = new List<string>();
    }
    #endregion
}
