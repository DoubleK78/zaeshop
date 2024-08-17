using Common.Models;

namespace Common.Interfaces;

public interface ISimpleTokenService
{
    bool VerifyToken<T>(string token, out T? payload, int limitMinutes = 1) where T : SimpleTokenPayload;
}
