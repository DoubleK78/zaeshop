using Portal.Domain.Models.LevelModels;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface ILevelService
    {
        Task AddExperienceFromUserToRedisAsync(LevelBuildRedisRequestModel model);
        Task CalculateExperiencesFromRedisTaskAsync();
        Task CalculateExperiencesFromRedisAsync();
        Task ResetJobNotUpdateRunningStatus();
    }
}
