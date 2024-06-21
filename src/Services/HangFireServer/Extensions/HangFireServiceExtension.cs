using Hangfire;
using Hangfire.SqlServer;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.SeedWork;
using static Common.ValueObjects.Const;

namespace HangFireServer.Extensions
{
    public static class HangFireServiceExtension
    {
        public static IServiceCollection AddHangFireServices(this IServiceCollection services, IConfiguration config)
        {
            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(config.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    JobExpirationCheckInterval = TimeSpan.FromDays(2),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                })
                .WithJobExpirationTimeout(TimeSpan.FromDays(2)));

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            return services;
        }

        public static void StartHangFireJobs(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            unitOfWork.ExecuteAsync("Hangfire_ResetJobs");

            // 10 Minutes to calculate views from redis
            RecurringJob.AddOrUpdate<ICollectionService>(HangfireJobName.CalculateViewsFromRedis, x => x.CalculateViewsFromRedisTaskAsync(), "*/10 * * * *");

            // 30 Minutes to calculate exps from redis
            RecurringJob.AddOrUpdate<ILevelService>(HangfireJobName.CalculateExperiencesFromRedis, x => x.CalculateExperiencesFromRedisTaskAsync(), "15,45 * * * *");

            // 12 Hours to send email noti
            RecurringJob.AddOrUpdate<IEmailService>(HangfireJobName.SendEmailSPremiumFollowers, x => x.SendEmailToFollowersTaskAsync(), "0 */12 * * *");

            // 1 Hours to reset role user
            RecurringJob.AddOrUpdate<IUserService>(HangfireJobName.ResetRoleUsers, x => x.ResetRoleTaskAsync(), "5 */1 * * *");
        
            // 5 Minutes to check hangfire job is still running start StartOnUtc > endOnUtc + 5m then stop running status
            RecurringJob.AddOrUpdate<ILevelService>(HangfireJobName.ResetJobNotUpdateRunningStatus, x => x.ResetJobNotUpdateRunningStatus(), "*/5 * * * *");

            // 30 Minutes to reset level public
            RecurringJob.AddOrUpdate<IAlbumService>(HangfireJobName.ResetLevelPublic, x => x.ResetLevelPublicTaskAsync(), "*/30 * * * *");

            // 30 Minutes to reset level public
            RecurringJob.AddOrUpdate<ICollectionService>(HangfireJobName.ResetLevelPublicChap, x => x.ResetLevelPublicTaskAsync(), "*/30 * * * *");
        
            // 10:05 AM ICT Daily To remind subscription
            RecurringJob.AddOrUpdate<IUserService>(HangfireJobName.RemindSubscription, x => x.RemindSubscriptionTaskAsync(), "5 3 * * *");
        
            // 7:05 AM ICT Daily to Recalculate All Album Views
            RecurringJob.AddOrUpdate<ICollectionService>(HangfireJobName.AlbumCalculatemViewsTopTypeAll, x => x.RecalculateAlbumViewsTopTypeAllTaskAsync(), "5 0 * * *");
        
            // 4:05 AM Hangfire Clean Jobs (All states)
            RecurringJob.AddOrUpdate<IActivityLogService>(HangfireJobName.HangfireCleanupJobs, x => x.CleanJobsHangfireAsync(), "5 21 * * *");
        }
    }
}