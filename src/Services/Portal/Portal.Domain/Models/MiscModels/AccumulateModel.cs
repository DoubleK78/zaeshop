using Common.Models;

namespace Portal.Domain.Models.MiscModels;

public class AccumulateModel
{
    public string Payload { get; set; } = null!;
}

public class AccumulatePayload : SimpleTokenPayload
{
    public bool IsBot { get; set; }

    public int CollectionId { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public DateTime RequestedOnUtc { get; set; }

    public int? PreviousCollectionId { get; set; }
}