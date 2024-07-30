using Portal.Domain.Enums;

namespace Portal.Domain.Models.AlbumModels
{
    public class AlbumScheduleResponseModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? BackgroundUrl { get; set; }
        public string? Url { get; set; }
        public bool Status { get; set; }
        public EDate DateRelease { get; set; }
        public string? Type { get; set; }
        public string? TimeRelease { get; set; }
        public ERegion Region { get; set; }
    }

    public class AlbumScheduleRequestModel
    {
        public EDate DateRelease { get; set; }
        public ERegion Region { get; set; }
    }

    public class AlbumScheduleModel
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
