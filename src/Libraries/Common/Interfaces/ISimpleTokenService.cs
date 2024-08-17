using Common.Models;

namespace Common.Interfaces;

public interface ISimpleTokenService
{
    bool VerifyToken<T>(string token, out T? payload) where T : SimpleTokenPayload;
}
