using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Portal.Domain.Enums;

namespace Portal.Domain.Models.AlbumModels
{
    public class AlbumRequestModel
    {
        [Required(ErrorMessage = "error_album_name_is_required")]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? OriginalUrl { get; set; }
        public int? AlbumAlertMessageId { get; set; }
        public List<int>? ContentTypeIds { get; set; }

        public bool? IsPublic { get; set; }
    }

    public class AlbumResponseModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public int? AlbumAlertMessageId { get; set; }
        public string? AlbumAlertMessageName { get; set; }

        public List<int>? ContentTypeIds { get; set; }
        public string? ContentTypeNames { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public bool IsPublic { get; set; }
    }

    public class AlbumPagingResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public int? AlbumAlertMessageId { get; set; }
        public string? AlbumAlertMessageName { get; set; }

        public string? ContentTypeIds { get; set; }
        public string? ContentTypes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        [JsonIgnore]
        public long RowNum { get; set; }

        [JsonIgnore]
        public bool IsTotalRecord { get; set; }
    }

    public class AlbumExtraInfoModel
    {
        public int? Id { get; set; }

        public string? AlternativeName { get; set; }
        public string? Type { get; set; }
        public EAlbumStatus AlbumStatus { get; set; }
        public string? ReleaseYear { get; set; }
        public string? AuthorNames { get; set; }
        public string? ArtistNames { get; set; }
        public string? Tags { get; set; }
    }
}
