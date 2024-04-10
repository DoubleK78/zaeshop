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

            var originalBanned = user.IsBanned;
            user.IsBanned = userModel.IsBanned;
            user.LockoutEnd = userModel.IsBanned ? DateTime.UtcNow : null;

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

            var userFingerPrints = _context.UserFingerPrints.Where(o => o.UserId == user.Id).ToList();
            if (originalBanned != user.IsBanned)
            {
                userFingerPrints.ForEach(x => x.IsBanned = user.IsBanned);
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

        public async Task<PagingCommonResponse<UserPaging>> GetPagingAsync(PagingCommonRequest model)
        {
            const string query = "User_All_Paging";
            var parameters = new DynamicParameters(
            new
            {
                model.PageNumber,
                model.PageSize,
                model.SearchTerm,
                model.SortColumn,
                model.SortDirection
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

        public async Task<ServiceResponse<UserRoleSubcriptionModel>> GetUserRoleSubcriptionAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<UserRoleSubcriptionModel>("error_user_not_found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRoleType = CommonHelper.GetRoleType([.. roles]);

            return new ServiceResponse<UserRoleSubcriptionModel>(new UserRoleSubcriptionModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.UserName,
                Avatar = user.Avatar,
                Role = CommonHelper.GetDescription(currentRoleType),
                ExpriedRoleDate = user.ExpriedRoleDate,
                CreatedOnUtc = user.CreatedOnUtc
            });
        }

        public async Task<string?> UpdateUserRoleFromSubscriptionAsync(UserRoleSubcriptionRequestModel requestModel)
        {
            var user = await _userManager.FindByIdAsync(requestModel.UserId);
            if (user == null)
            {
                return "error_user_not_found";
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentUserRole = CommonHelper.GetRoleType([.. userRoles]);
            var newRoleType = CommonHelper.GetRoleType([requestModel.Role]);

            #region Validate
            // Add new roles
            var isValidRole = await _roleManager.Roles.AnyAsync(r => r.Name == requestModel.Role);
            if (!isValidRole)
            {
                return "error_role_not_found";
            }

            // Three roles in subscription, other roles that we can use Edit User Feature
            if (newRoleType != ERoleType.User && newRoleType != ERoleType.UserPremium && newRoleType != ERoleType.UserSuperPremium)
            {
                return "error_role_not_in_subscription";
            }

            // User can upgrade but not support downgrade
            if (newRoleType < currentUserRole)
            {
                return "error_upgrade_not_support_downgrade";
            }
            #endregion

            // Remove current roles
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            await _userManager.AddToRolesAsync(user, [requestModel.Role]);

            // Update expries role for user
            // Case 1: New role subscription from user -> DateTime.UtcNow + days
            // Case 2: Same role subscription -> Increase expries date
            // Case 3: New role subscription from user premium to user super premium
            // Calculate and convert expries date of current role user premium = 10 % remaining days and add days
            if (newRoleType != ERoleType.User && requestModel.Days.HasValue)
            {
                // Case 1
                if (currentUserRole == ERoleType.User)
                {
                    user.ExpriedRoleDate = DateTime.UtcNow.AddDays(requestModel.Days.Value);
                }
                // Case 2
                else if (currentUserRole == newRoleType)
                {
                    if (user.ExpriedRoleDate.HasValue)
                    {
                        user.ExpriedRoleDate = user.ExpriedRoleDate.Value.AddDays(requestModel.Days.Value);
                    }
                    else
                    {
                        user.ExpriedRoleDate = DateTime.UtcNow.AddDays(requestModel.Days.Value);
                    }
                }
                // Case 3
                else if (currentUserRole == ERoleType.UserPremium && newRoleType == ERoleType.UserSuperPremium && user.ExpriedRoleDate.HasValue)
                {
                    var remainDays = Math.Abs(DateTime.UtcNow.Subtract(user.ExpriedRoleDate.Value).Days);
                    // Assume ExpriedRoleDate is expired then remain will be zero
                    var trueRemainDays = remainDays > 0 ? remainDays : 0;

                    user.ExpriedRoleDate = DateTime.UtcNow.AddDays((double)requestModel.Days + trueRemainDays * 0.1);
                }
            }
            else
            {
                user.ExpriedRoleDate = null;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Sync to Portal and logs
            await _syncRolesPortalPublisher.SyncRolesPortalAsync(new SyncRolesPortalMessage
            {
                Days = requestModel.Days,
                IsUpdateSubscription = true,
                IdentityUserId = user.Id,
                Roles = [requestModel.Role],
                OldRoleType = currentUserRole,
                NewRoleType = newRoleType,
                ExpriedRoleDate = user.ExpriedRoleDate
            });

            return string.Empty;
        }
    }
}