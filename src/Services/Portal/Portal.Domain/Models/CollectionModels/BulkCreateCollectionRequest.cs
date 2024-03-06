using Common.Enums;
using Portal.Domain.AggregatesModel.CollectionAggregate;

namespace Portal.Domain.Models.CollectionModels
{
    public class BulkCreateCollectionRequest
    {
        public string Name { get; set; } = null!;
        public bool IsPriority { get; set; }
        public EStorageType StorageType { get; set; }
        public List<BulkCreateCollectionItemRequest> ContentItems { get; set; } = null!;
    }

    public class BulkCreateCollectionItemRequest
    {
        public string Name { get; set; } = null!;
        public int Size { get; set; }
    }

    public class BulkCreateCollectionDb
    {
        public Collection Collection { get; set; } = null!;
        public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    }
}
