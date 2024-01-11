using Portal.Domain.Models.ImageUploadModels;

namespace Portal.Domain.Interfaces.External
{
    public interface IAmazonS3Service
    {
        Task<ImageUploadResultModel> UploadImageAsync(ImageUploadRequestModel requestModel, string? prefix = null);
        Task<List<ImageUploadResultModel>> BulkUploadImagesAsync(List<ImageUploadRequestModel> requestModels, string? prefix = null);
    }
}
