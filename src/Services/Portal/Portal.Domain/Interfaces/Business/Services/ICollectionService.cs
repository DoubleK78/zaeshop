using Common.Models;
using Portal.Domain.Models.CollectionModels;
using Portal.Domain.Models.ContentItemModels;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface ICollectionService
    {
        Task<ServiceResponse<CollectionResponseModel>> CreateAsync(CollectionRequestModel requestModel);
        Task<ServiceResponse<CollectionResponseModel>> UpdateAsync(int id, CollectionRequestModel requestModel);
        Task<ServiceResponse<List<CollectionResponseModel>>> GetAllAsync();
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<List<GetContentItemModel>>> GetContentItemsAsync(int id);
        Task<ServiceResponse<PagingCommonResponse<CollectionPagingResponse>>> GetPagingAsync(CollectionPagingRequest request);
        Task AddViewFromUserToRedisAsync(CollectionViewUserBuildModel model);
        Task CalculateViewsFromRedisTaskAsync();
        Task<ServiceResponse<CollectionResponseModel>> GetByIdAsync(int id);
        Task<ServiceResponse<string>> BulkCreateAsync(int albumId, List<BulkCreateCollectionRequest> collections);
        Task CaclculateViewsFromRedisAsync();
        Task ResetLevelPublicTaskAsync();
        Task PushNotificationComic(int albumId, bool isPriority, string title, string description, string clickAction);
        Task BuildCacheComicDetails(List<string?> comicFriendlyNames);
        Task RecalculateAlbumViewsTopTypeAsync(List<int> collectionIds);
        Task RecalculateAlbumViewsTopTypeAllTaskAsync();
        Task RecalculateAlbumViewsTopTypeAllAsync();
    }
}
