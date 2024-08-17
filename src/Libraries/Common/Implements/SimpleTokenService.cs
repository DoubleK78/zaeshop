using System.Security.Cryptography;
using System.Text;
using Common.Interfaces;
using Common.Models;

namespace Common.Implements;

public class SimpleTokenService : ISimpleTokenService
{
    private readonly string _secretKey;

    public SimpleTokenService(string secretKey)
    {
        _secretKey = secretKey.ToLower();
    }

    public bool VerifyToken<T>(string token, out T? payload) where T : SimpleTokenPayload
    {
        payload = null;
        var parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        string base64Payload = parts[0];
        string signature = parts[1];

        if (signature != ComputeSignature(base64Payload))
            return false;

        string jsonPayload = Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload));

        try
        {
            payload = JsonSerializationHelper.Deserialize<T?>(jsonPayload);
            if (payload == null)
                return false;
        }
        catch
        {
            return false;
        }

        long timestamp = payload.Timestamp;
        long expiresIn = payload.ExpiresIn;

        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= timestamp + expiresIn;
    }

    private string ComputeSignature(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }
}
