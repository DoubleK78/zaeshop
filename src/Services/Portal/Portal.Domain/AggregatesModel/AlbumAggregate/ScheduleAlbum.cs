using Portal.Domain.Enums;
using Portal.Domain.SeedWork;

namespace Portal.Domain.AggregatesModel.AlbumAggregate
{
    public class ScheduleAlbum : Entity
    {
        public string Title { get; set; } = null!;
        public string BackgroundUrl { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool Status { get; set; }
        public EDate DateRelease { get; set; }
        public string Type { get; set; } = null!;
        public string TimeRelease { get; set; } = null!;
        public ERegion Region { get; set; }
    }
}
