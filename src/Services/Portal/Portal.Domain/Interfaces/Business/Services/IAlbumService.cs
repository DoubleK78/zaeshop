using Common.Models;
using Portal.Domain.Models.AlbumModels;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface IAlbumService
    {
        Task<ServiceResponse<AlbumResponseModel>> CreateAsync(AlbumRequestModel requestModel);
        Task<ServiceResponse<AlbumResponseModel>> UpdateAsync(int id, AlbumRequestModel requestModel);
        Task<ServiceResponse<List<AlbumResponseModel>>> GetAllAsync();
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetPagingAsync(PagingCommonRequest request);
        Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetPagingByELKAsync(PagingCommonRequest request);
    }
}
