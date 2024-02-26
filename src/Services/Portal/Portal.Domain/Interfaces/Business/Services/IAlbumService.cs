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
        Task<ServiceResponse<PagingCommonResponse<AlbumPagingResponse>>> GetPagingAsync(PagingCommonRequest request, FilterAdvanced filter);
        Task<ServiceResponse<AlbumResponseModel>> GetByIdAsync(int id);
        Task<ServiceResponse<AlbumExtraInfoModel>> GetExtraInfoByIdAsync(int id);
        Task<ServiceResponse<AlbumExtraInfoModel>> UpdateExtraInfoByIdAsync(int id, AlbumExtraInfoModel requestModel);
        Task ResetLevelPublicTaskAsync();
        Task<ServiceResponse<List<AlbumScheduleResponseModel>>> GetScheduleAsync(AlbumScheduleRequestModel requestModel);
        Task<ServiceResponse<bool>> CreateScheduleAsync(AlbumScheduleModel requestModel);
    }
}
