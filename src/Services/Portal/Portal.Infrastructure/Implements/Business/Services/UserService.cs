using Common.Enums;
using Common.Interfaces.Messaging;
using Common.Models;
using Common.Shared.Models.Logs;
using Common.Shared.Models.Users;
using Common.ValueObjects;
using Microsoft.Extensions.Hosting;
using Portal.Domain.AggregatesModel.TaskAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Interfaces.Messaging;
using Portal.Domain.Models.UserModels;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISyncResetExpiredRolePublisher _syncResetExpiredRolePublisher;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IHostEnvironment _hostingEnvironment;

        public UserService(
            IUnitOfWork unitOfWork,
            ISyncResetExpiredRolePublisher syncResetExpiredRolePublisher,
            IServiceLogPublisher serviceLogPublisher,
            IHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
            _syncResetExpiredRolePublisher = syncResetExpiredRolePublisher;
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task ResetRoleTaskAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            var scheduleJob = await _unitOfWork.Repository<HangfireScheduleJob>().GetByNameAsync(Const.HangfireJobName.ResetRoleUsers);
            if (scheduleJob != null && scheduleJob.IsEnabled && !scheduleJob.IsRunning)
            {
                try
                {
                    scheduleJob.IsRunning = true;
                    scheduleJob.StartOnUtc = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    await ResetRoleAsync();

                    scheduleJob.EndOnUtc = DateTime.UtcNow;
                    scheduleJob.IsRunning = false;
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                    {
                        LogLevel = ELogLevel.Error,
                        EventName = ex.Message,
                        StackTrace = ex.StackTrace,
                        ServiceName = "Hangfire",
                        Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                        Description = $"[Exception]: {ex.Message}",
                        StatusCode = "Internal Server Error"
                    });

                    scheduleJob.EndOnUtc = DateTime.UtcNow;
                    scheduleJob.IsRunning = false;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task ResetRoleAsync()
        {
            var allUserPre = await _userRepository.GetQueryable()
                .Where(x => (x.RoleType == ERoleType.UserPremium || x.RoleType == ERoleType.UserSuperPremium) && x.ExpriedRoleDate != null)
                .ToListAsync();
            var userExpiredRoleIds = allUserPre.ConvertAll(x => x.IdentityUserId);

            foreach (var user in allUserPre)
            {
                if (user.ExpriedRoleDate <= DateTime.UtcNow)
                {
                    user.RoleType = ERoleType.User;
                    user.ExpriedRoleDate = null;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Remove role from Identity and sync User
            await _syncResetExpiredRolePublisher.SendAsync(new SyncResetExpiredRoleMessage
            {
                UserIds = userExpiredRoleIds
            });
        }

        public async Task<ServiceResponse<PagingCommonResponse<UserPagingResponse>>> GetPagingAsync(PagingCommonRequest request, ERegion region)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "PageNumber", request.PageNumber },
                { "PageSize", request.PageSize },
                { "SearchTerm", request.SearchTerm },
                { "SortColumn", "CurrentExp" },
                { "SortDirection", "DESC" },
                { "Region", region }
            };
            var result = await _unitOfWork.QueryAsync<UserPagingResponse>("User_Ranking_All_Paging", parameters);

            var record = result.Find(o => o.IsTotalRecord);
            if (record == null)
            {
                return new ServiceResponse<PagingCommonResponse<UserPagingResponse>>(new PagingCommonResponse<UserPagingResponse>
                {
                    RowNum = 0,
                    Data = new List<UserPagingResponse>()
                });
            }

            result.Remove(record);
            return new ServiceResponse<PagingCommonResponse<UserPagingResponse>>(new PagingCommonResponse<UserPagingResponse>
            {
                RowNum = record.RowNum,
                Data = result
            });
        }
    }
}
