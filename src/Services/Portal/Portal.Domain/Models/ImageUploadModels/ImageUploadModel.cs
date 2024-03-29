namespace Portal.Domain.Models.ImageUploadModels
{
    public class ImageUploadRequestModel
    {
        public string FileName { get; set; } = null!;
        public byte[] ImageData { get; set; } = null!;

        public bool IsPublic { get; set; }
        public int OrderBy { get; set; }
    }

    public class ImageUploadResultModel
    {
        public string FileName { get; set; } = null!;
        public string? AbsoluteUrl { get; set; }
        public string? RelativeUrl { get; set; }

        public bool IsPublic { get; set; }
        public int OrderBy { get; set; }
    }
}
