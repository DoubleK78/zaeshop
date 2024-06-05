using Common.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.ActivityLogs;
using Portal.Domain.SeedWork;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserActivityLog> _activityRepository;
        private readonly IConfiguration _configuration;

        public ActivityLogService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _activityRepository = unitOfWork.Repository<UserActivityLog>();
            _configuration = configuration;
        }

        public async Task<ServiceResponse<bool>> CreateAsync(ActivityLogRequestModel requestModel)
        {
            if (requestModel == null || requestModel.ActivityType == null || requestModel.UserId == null)
                return new ServiceResponse<bool>("error_log_activity");

            var logLastTimesInDay = await _activityRepository.GetQueryable()
                .Where(x => x.CreatedOnUtc.Date == DateTime.UtcNow.Date && x.ActivityType == EActivityType.Comment && x.UserId == requestModel.UserId)
                .OrderByDescending(x => x.LogTimes)
                .FirstOrDefaultAsync();

            var entity = new UserActivityLog()
            {
                ActivityType = requestModel.ActivityType ?? EActivityType.Comment,
                Description = requestModel.Description,
                CreatedOnUtc = DateTime.UtcNow,
                IpV4Address = requestModel.IpV4Address,
                IpV6Address = requestModel.IpV6Address,
                UserId = requestModel.UserId.Value
            };

            if (logLastTimesInDay == null)
                entity.LogTimes = 1;
            else
            {
                if (logLastTimesInDay.LogTimes < requestModel.LimitTimes)
                    entity.LogTimes = logLastTimesInDay.LogTimes + 1;
                else
                    return new ServiceResponse<bool>("over_limit");
            }

            _activityRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse<bool>(true);
        }

        public async Task CleanJobsHangfireAsync()
        {
            string? connectionString = _configuration.GetConnectionString("HangfireConnection");

            if (!string.IsNullOrEmpty(connectionString))
            {
                const string query = "Hangfire_CleanupJobs";
                using var connection = new SqlConnection(connectionString);
                await connection.ExecuteAsync(query, commandType: CommandType.StoredProcedure, commandTimeout: 1800);
            }
        }
    }
}
