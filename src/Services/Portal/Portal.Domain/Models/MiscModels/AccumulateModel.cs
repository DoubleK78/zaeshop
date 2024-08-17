using Common.Models;

namespace Portal.Domain.Models.MiscModels;

public class AccumulateModel
{
    public string Token { get; set; } = null!;
}

public class AccumulatePayload : SimpleTokenPayload
{
    public bool IsBot { get; set; }

    public int CollectionId { get; set; }

    public string? CreatedOnUtc { get; set; }

    public int? PreviousCollectionId { get; set; }

    public DateTime GetCreatedOnUtc()
    {
        return DateTime.TryParse(CreatedOnUtc, out var dateTime) ? dateTime : DateTime.UtcNow;
    }
}