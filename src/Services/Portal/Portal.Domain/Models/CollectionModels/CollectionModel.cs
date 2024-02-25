using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Models;
using Portal.Domain.Enums;

namespace Portal.Domain.Models.CollectionModels
{
    // Request model for Collection
    public class CollectionRequestModel
    {
        [Required(ErrorMessage = "error_collection_title_is_required")]
        public string Title { get; set; } = null!;

        public int AlbumId { get; set; }

        public int? Volume { get; set; }

        public string? ExtendName { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// Priority for Premium User
        /// </summary>
        public bool IsPriority { get; set; }
    }

    public class ContentItemRequestModel
    {
        public List<ContentItemRequestDetailModel>? Items { get; set; }
    }

    public class ContentItemRequestDetailModel
    {
        public string Name { get; set; } = null!;
        public byte[] Data { get; set; } = null!;
    }

    // Response model for Collection
    public class CollectionResponseModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public string? AlbumFriendlyName { get; set; }
        public int? Volume { get; set; }
        public string? ExtendName { get; set; }
        public string? Description { get; set; }
        public string? FriendlyName { get; set; }

        public List<string?>? ContentItems { get; set; }
    }

    public class CollectionPagingRequest : PagingCommonRequest
    {
        public int AlbumId { get; set; }
    }

    public class CollectionPagingResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public int? Volume { get; set; }
        public string? ExtendName { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public ulong Views { get; set; }
        public ELevelPublic LevelPublic { get; set; }

        [JsonIgnore]
        public long RowNum { get; set; }

        [JsonIgnore]
        public bool IsTotalRecord { get; set; }
    }
}
