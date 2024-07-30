using Common.Models;
using Portal.Domain.Enums;
using Portal.Domain.Models.UserModels;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface IUserService
    {
        Task ResetRoleTaskAsync();
        Task ResetRoleAsync();
        Task<ServiceResponse<PagingCommonResponse<UserPagingResponse>>> GetPagingAsync(PagingCommonRequest request, ERegion region);
    }
}
