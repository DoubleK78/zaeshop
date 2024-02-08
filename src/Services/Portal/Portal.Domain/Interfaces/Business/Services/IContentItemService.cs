using Common.Models;
using Portal.Domain.Models.ContentItemModels;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface IContentItemService
    {
        Task<ServiceResponse<bool>> CreateContentItemsAsync(int collectionId, Stream stream, string contentType);
        Task<ServiceResponse<bool>> UpdateContentItemsAsync(int collectionId, ContentItemUpdateRequestModel model);
        Task<ServiceResponse<bool>> BulkUploadByLocalServer(int collectionId, List<ContentItemUploadLocalServer> items);
        Task<ServiceResponse<bool>> BulkUpdateByLocalServer(int collectionId, List<ContentItemUploadLocalServer> items);
    }
}
