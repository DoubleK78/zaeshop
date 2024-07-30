using Common.Models;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Identity.Domain.Interfaces.Infrastructure
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user, int expirationInMinutes = 60);
        UserInfomationTokenModel? ValidateJwtToken(string token);
        UserToken GenerateRefreshToken(string ipAddress);
    }
}