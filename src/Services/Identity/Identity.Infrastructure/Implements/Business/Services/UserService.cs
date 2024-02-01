using System.Data;
using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Models;
using Common.Shared.Models.Users;
using Dapper;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Business.Interfaces.Services;
using Identity.Domain.Interfaces.Messaging;
using Identity.Domain.Models.ErrorCodes;
using Identity.Domain.Models.ErrorResponses;
using Identity.Domain.Models.Roles;
using Identity.Domain.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Implements.Business.Services
{
    public class UserService : IUserService
    {
        private readonly AppIdentityDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IApiService _apiService;
        private readonly ISyncUserPortalPublisher _syncUserPortalPublisher;
        private readonly ISyncRolesPortalPublisher _syncRolesPortalPublisher;

        public UserService(
            AppIdentityDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IApiService apiService,
            ISyncUserPortalPublisher syncUserPortalPublisher,
            ISyncRolesPortalPublisher syncRolesPortalPublisher)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _apiService = apiService;
            _syncUserPortalPublisher = syncUserPortalPublisher;
            _syncRolesPortalPublisher = syncRolesPortalPublisher;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(o => o.Id == id);
            return user;
        }

        public async Task<UserRegisterResponseModel?> CreateAsync(UserRegisterRequestModel userModel, ErrorResult errorResult)
        {
            var existsUser = await _userManager.FindByNameAsync(userModel.Email);
            if (existsUser != null)
            {
                errorResult.Description = nameof(ErrorCodes.UserNotExists);
                return null;
            }

            var user = new User
            {
                UserName = userModel.Username,
                Email = userModel.Email,
                FullName = userModel.FullName,
                VerifiedOnUtc = DateTime.UtcNow,
                CreatedOnUtc = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);
            if (!result.Succeeded)
            {
                errorResult.Description = string.Join(", ", result.Errors.Select(o => o.Description));
                return null;
            }

            // Sync to portal
            var resultApi = await _apiService.PostAsync<SyncUserFromIdentityRequestModel, SyncUserFromIdentityResponseModel>(CommonHelper.GetServiceUrl(EServiceHost.Portal), "/v1/users", new SyncUserFromIdentityRequestModel
            {
                IdentityId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName
            });

            if (resultApi != null && !resultApi.IsSuccess)
            {
                await _userManager.DeleteAsync(user);
                errorResult.Description = resultApi.Message ?? string.Empty;
                return null;
            }

            return new UserRegisterResponseModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                UserName = user.UserName
            };
        }

        public async Task<UserRegisterResponseModel?> UpdateAsync(string id, UserUpdateRequestModel userModel, ErrorResult errorResult)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                errorResult.Description = nameof(ErrorCodes.UserNotExists);
                return null;
            }

            bool isSyncUserPortal = user.FullName != userModel.FullName;
            user.FullName = userModel.FullName;

            if (!string.IsNullOrEmpty(userModel.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userModel.Password);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                errorResult.Description = string.Join(", ", result.Errors.Select(o => o.Description));
                return null;
            }

            // Update role
            if (userModel.Roles?.Count > 0)
            {
                var allRoles = _roleManager.Roles.ToList();
                var dbRoles = allRoles.ConvertAll(o => o.Name);

                var isValidRoles = userModel.Roles.TrueForAll(dbRoles.Contains);
                if (!isValidRoles)
                {
                    errorResult.Description = "error_role_is_invalid";
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                var rolesToAdd = userModel.Roles.Except(userRoles);
                var rolesToRemove = userRoles.Except(userModel.Roles);

                await _userManager.AddToRolesAsync(user, rolesToAdd);
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }
            else
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                }
            }

            // Sync user portal
            if (isSyncUserPortal)
            {
                await _syncUserPortalPublisher.SyncUserPortalAsync(new SyncUserPortalMessage
                {
                    IdentityUserId = user.Id,
                    FullName = user.FullName
                });
            }

            // Sync roles for user portal
            await _syncRolesPortalPublisher.SyncRolesPortalAsync(new SyncRolesPortalMessage
            {
                IdentityUserId = user.Id,
                Roles = userModel.Roles
            });

            return new UserRegisterResponseModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                UserName = user.UserName
            };
        }

        public async Task<PagingCommonResponse<UserPaging>> GetPagingAsync(int pageNumber, int pageSize)
        {
            const string query = "User_All_Paging";
            var parameters = new DynamicParameters(
            new
            {
                pageNumber,
                pageSize
            });
            var result = (await _context.Database.GetDbConnection().QueryAsync<UserPaging>(query, parameters, commandType: CommandType.StoredProcedure)).ToList();
            var record = result.Find(o => o.IsTotalRecord);

            if (record == null)
            {
                return new PagingCommonResponse<UserPaging>
                {
                    RowNum = 0,
                    Data = new List<UserPaging>()
                };
            }

            result.Remove(record);
            return new PagingCommonResponse<UserPaging>
            {
                RowNum = record.RowNum,
                Data = result
            };
        }

        public async Task<PagingCommonResponse<RolePaging>> GetRolesPagingAsync(int pageNumber, int pageSize)
        {
            const string query = "Role_All_Paging";
            var parameters = new DynamicParameters(
            new
            {
                pageNumber,
                pageSize
            });
            var result = (await _context.Database.GetDbConnection().QueryAsync<RolePaging>(query, parameters, commandType: CommandType.StoredProcedure)).ToList();
            var record = result.Find(o => o.IsTotalRecord);

            if (record == null)
            {
                return new PagingCommonResponse<RolePaging>
                {
                    RowNum = 0,
                    Data = new List<RolePaging>()
                };
            }

            result.Remove(record);
            return new PagingCommonResponse<RolePaging>
            {
                RowNum = record.RowNum,
                Data = result
            };
        }

        public async Task<User?> GetByProviderAccountIdAsync(string providerAccountId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(o => o.ProviderAccountId == providerAccountId);
            return user;
        }
    }
}