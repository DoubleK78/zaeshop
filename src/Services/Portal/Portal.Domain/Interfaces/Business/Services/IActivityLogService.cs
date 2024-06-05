using Common.Models;
using Portal.Domain.Models.ActivityLogs;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface IActivityLogService
    {
        Task<ServiceResponse<bool>> CreateAsync(ActivityLogRequestModel requestModel);
        Task CleanJobsHangfireAsync();
    }
}
