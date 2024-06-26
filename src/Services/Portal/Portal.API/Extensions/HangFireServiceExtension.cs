using Hangfire;
using Hangfire.SqlServer;

namespace Portal.API.Extensions
{
    public static class HangFireServiceExtension
    {
        public static IServiceCollection AddHangFireServices(this IServiceCollection services, IConfiguration config)
        {
            // Add Hangfire services.
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            if (!isDeployed)
            {
                services.AddHangfire(configuration => configuration
                               .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
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
            }
            else
            {
                // Add Hangfire services - In memory
                GlobalConfiguration.Configuration.UseInMemoryStorage();
                services.AddHangfire(x => x.UseInMemoryStorage());
                services.AddHangfireServer();
            }

            return services;
        }
    }
}