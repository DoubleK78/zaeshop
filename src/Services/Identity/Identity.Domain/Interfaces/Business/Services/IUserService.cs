using Common.Models;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Models.ErrorResponses;
using Identity.Domain.Models.Roles;
using Identity.Domain.Models.Users;

namespace Identity.Domain.Business.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<UserRegisterResponseModel?> CreateAsync(UserRegisterRequestModel userModel, ErrorResult errorResult);
        Task<UserRegisterResponseModel?> UpdateAsync(string id, UserUpdateRequestModel userModel, ErrorResult errorResult);
        Task<PagingCommonResponse<UserPaging>> GetPagingAsync(PagingCommonRequest model);
        Task<PagingCommonResponse<RolePaging>> GetRolesPagingAsync(int pageNumber, int pageSize);
        Task<User?> GetByProviderAccountIdAsync(string providerAccountId);
        Task<ServiceResponse<UserRoleSubcriptionModel>> GetUserRoleSubcriptionAsync(string userId);
        Task<string?> UpdateUserRoleFromSubscriptionAsync(UserRoleSubcriptionRequestModel requestModel);
    }
}