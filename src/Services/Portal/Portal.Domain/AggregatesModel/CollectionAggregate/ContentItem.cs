using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Portal.Domain.Enums;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.CollectionAggregate
{
    public class ContentItem : Entity
    {
        public int CollectionId { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Absolute url
        /// </summary>
        [Column(TypeName = "varchar(550)")]
        public string? OriginalUrl { get; set; }

        /// <summary>
        /// Absolute or CDN url
        [Column(TypeName = "varchar(250)")]
        /// </summary>
        public string? DisplayUrl { get; set; }

        /// <summary>
        /// File Name or Relative Url
        /// </summary>
        [Column(TypeName = "varchar(550)")]
        public string? RelativeUrl { get; set; }

        /// <summary>
        /// Image order number
        /// </summary>
        public int OrderBy { get; set; }

        public EContentItemType Type { get; set; }
        public bool IsPublic { get; set; }

        [JsonIgnore]
        public virtual Collection Collection { get; set; } = null!;
    }
}
