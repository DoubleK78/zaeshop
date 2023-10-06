using Common.Models;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Models.ErrorResponses;
using Identity.Infrastructure.Models.Roles;
using Identity.Infrastructure.Models.Users;

namespace Identity.Infrastructure.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<UserRegisterResponseModel?> CreateAsync(UserRegisterRequestModel userModel, ErrorResult errorResult);
        Task<UserRegisterResponseModel?> UpdateAsync(string id, UserUpdateRequestModel userModel, ErrorResult errorResult);
        Task<PagingCommonResponse<UserPaging>> GetPagingAsync(int pageNumber, int pageSize);
        Task<PagingCommonResponse<RolePaging>> GetRolesPagingAsync(int pageNumber, int pageSize);
    }
}