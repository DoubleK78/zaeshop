using Common.Enums;
using Common.Interfaces.Messaging;
using Common.Models;
using Common.Shared.Models.Logs;
using Common.Shared.Models.Users;
using Common.ValueObjects;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Portal.Domain.AggregatesModel.TaskAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Interfaces.External;
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
        private readonly IBackgroundJobClient _backgroundJobClient;

        public UserService(
            IUnitOfWork unitOfWork,
            ISyncResetExpiredRolePublisher syncResetExpiredRolePublisher,
            IServiceLogPublisher serviceLogPublisher,
            IHostEnvironment hostingEnvironment,
            IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
            _syncResetExpiredRolePublisher = syncResetExpiredRolePublisher;
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
            _backgroundJobClient = backgroundJobClient;
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
                    user.RemindSubscription = ERemindSubscription.None;
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

        public async Task RemindSubscriptionTaskAsync()
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;

            var scheduleJob = await _unitOfWork.Repository<HangfireScheduleJob>().GetByNameAsync(Const.HangfireJobName.RemindSubscription);
            if (scheduleJob != null && scheduleJob.IsEnabled && !scheduleJob.IsRunning)
            {
                try
                {
                    var parameters = new Dictionary<string, object?>
                    {
                        { "Id",  scheduleJob.Id }
                    };
                    await _unitOfWork.ExecuteAsync("Hangfire_StartJob", parameters);

                    await RemindSubscriptionAsync();

                    await _unitOfWork.ExecuteAsync("Hangfire_EndJob", parameters);
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

                    var parameters = new Dictionary<string, object?>
                    {
                        { "Id",  scheduleJob.Id }
                    };
                    await _unitOfWork.ExecuteAsync("Hangfire_EndJob", parameters);
                }
            }
        }

        public async Task RemindSubscriptionAsync()
        {
            // Get all user pre with expried date
            var next7days = DateTime.UtcNow.AddDays(7);
            var allUserPre = await _userRepository.GetQueryable()
                .Where(x => (x.RoleType == ERoleType.UserPremium || x.RoleType == ERoleType.UserSuperPremium) &&
                    x.ExpriedRoleDate.HasValue && x.ExpriedRoleDate.Value <= next7days)
                .ToListAsync();
            var allUserPreIds = allUserPre.ConvertAll(x => x.Id);

            var userDevices = await _unitOfWork.Repository<UserDevice>()
                                .GetQueryable()
                                .Where(o => allUserPreIds.Contains(o.UserId) && o.IsEnabled)
                                .ToListAsync();

            #region Push Notification
            // Remind user will expried in 7 days
            var usersRemind7Days = allUserPre.Where(x => x.RemindSubscription != ERemindSubscription.SevenDays &&
                x.ExpriedRoleDate.HasValue &&
                Math.Abs(DateTime.UtcNow.Subtract(x.ExpriedRoleDate.Value).TotalDays) > 3).ToList();

            if (usersRemind7Days.Count > 0)
            {
                // Devide users by Region
                var usersGroupByRegion = usersRemind7Days.GroupBy(x => new { x.Region });
                foreach (var usersRegion in usersGroupByRegion)
                {
                    var region = usersRegion.Key.Region;
                    if (region == ERegion.vi)
                    {
                        var usersVi = usersRegion.ToList();
                        var usersViIds = usersVi.ConvertAll(x => x.Id);
                        var userDeviceOfVi = userDevices.Where(x => usersViIds.Contains(x.UserId)).ToList();

                        if (usersVi.Count > 0 && userDeviceOfVi.Count > 0)
                        {
                            var titlePushNotification = Const.PushNotification.RemindSubscriptionVi;
                            var descriptionPushNotification = string.Format(Const.PushNotification.RemindSubscriptionDescriptionVi, 7);
                            var clickActionPushNotification = "/nang-cap-goi";

                            _backgroundJobClient.Schedule<IFirebaseCloudMessageService>(x => x.SendAllAsync(
                               userDeviceOfVi.Select(u => u.RegistrationToken).Distinct().ToList(),
                               titlePushNotification,
                               descriptionPushNotification,
                               clickActionPushNotification
                           ), TimeSpan.FromMinutes(1));
                        }
                    }
                    else if (region == ERegion.en)
                    {
                        var usersEn = usersRegion.ToList();
                        var usersEnIds = usersEn.ConvertAll(x => x.Id);
                        var userDeviceOfEn = userDevices.Where(x => usersEnIds.Contains(x.UserId)).ToList();

                        if (usersEn.Count > 0 && userDeviceOfEn.Count > 0)
                        {
                            var titlePushNotification = Const.PushNotification.RemindSubscriptionEn;
                            var descriptionPushNotification = string.Format(Const.PushNotification.RemindSubscriptionDescriptionEn, 7);
                            var clickActionPushNotification = "/en/upgrade-package";

                            _backgroundJobClient.Schedule<IFirebaseCloudMessageService>(x => x.SendAllAsync(
                               userDeviceOfEn.Select(u => u.RegistrationToken).Distinct().ToList(),
                               titlePushNotification,
                               descriptionPushNotification,
                               clickActionPushNotification
                           ), TimeSpan.FromMinutes(1));
                        }
                    }
                }
            }

            // Remind user will expried in 3 days
            var usersRemind3Days = allUserPre.Where(x => x.RemindSubscription != ERemindSubscription.ThreeDays &&
               x.ExpriedRoleDate.HasValue &&
               Math.Abs(DateTime.UtcNow.Subtract(x.ExpriedRoleDate.Value).TotalDays) <= 3).ToList();

            if (usersRemind3Days.Count > 0)
            {
                // Devide users by Region
                var usersGroupByRegion = usersRemind3Days.GroupBy(x => new { x.Region });
                foreach (var usersRegion in usersGroupByRegion)
                {
                    var region = usersRegion.Key.Region;
                    if (region == ERegion.vi)
                    {
                        var usersVi = usersRegion.ToList();
                        var usersViIds = usersVi.ConvertAll(x => x.Id);
                        var userDeviceOfVi = userDevices.Where(x => usersViIds.Contains(x.UserId)).ToList();

                        if (usersVi.Count > 0 && userDeviceOfVi.Count > 0)
                        {
                            var titlePushNotification = Const.PushNotification.RemindSubscriptionVi;
                            var descriptionPushNotification = string.Format(Const.PushNotification.RemindSubscriptionDescriptionVi, 3);
                            var clickActionPushNotification = "/nang-cap-goi";

                            _backgroundJobClient.Schedule<IFirebaseCloudMessageService>(x => x.SendAllAsync(
                               userDeviceOfVi.Select(u => u.RegistrationToken).Distinct().ToList(),
                               titlePushNotification,
                               descriptionPushNotification,
                               clickActionPushNotification
                           ), TimeSpan.FromMinutes(1));
                        }
                    }
                    else if (region == ERegion.en)
                    {
                        var usersEn = usersRegion.ToList();
                        var usersEnIds = usersEn.ConvertAll(x => x.Id);
                        var userDeviceOfEn = userDevices.Where(x => usersEnIds.Contains(x.UserId)).ToList();

                        if (usersEn.Count > 0 && userDeviceOfEn.Count > 0)
                        {
                            var titlePushNotification = Const.PushNotification.RemindSubscriptionEn;
                            var descriptionPushNotification = string.Format(Const.PushNotification.RemindSubscriptionDescriptionEn, 3);
                            var clickActionPushNotification = "/en/upgrade-package";

                            _backgroundJobClient.Schedule<IFirebaseCloudMessageService>(x => x.SendAllAsync(
                               userDeviceOfEn.Select(u => u.RegistrationToken).Distinct().ToList(),
                               titlePushNotification,
                               descriptionPushNotification,
                               clickActionPushNotification
                           ), TimeSpan.FromMinutes(1));
                        }
                    }
                }
            }
            #endregion

            #region Update Remind Subscription Status
            foreach (var user in allUserPre)
            {
                if (user.ExpriedRoleDate.HasValue && Math.Abs(DateTime.UtcNow.Subtract(user.ExpriedRoleDate.Value).TotalDays) > 3 && Math.Abs(DateTime.UtcNow.Subtract(user.ExpriedRoleDate.Value).TotalDays) <= 7)
                {
                    user.RemindSubscription = ERemindSubscription.SevenDays;
                }
                else if (user.ExpriedRoleDate.HasValue && Math.Abs(DateTime.UtcNow.Subtract(user.ExpriedRoleDate.Value).TotalDays) <= 3)
                {
                    user.RemindSubscription = ERemindSubscription.ThreeDays;
                }
            }
            #endregion

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
