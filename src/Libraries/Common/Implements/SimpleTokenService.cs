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

    public bool VerifyToken<T>(string token, out T? payload, int limitMinutes = 2) where T : SimpleTokenPayload
    {
        payload = null;
        var parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        string base64Payload = parts[0];
        string signature = parts[1];

        // Compute the expected signature in hexadecimal format
        string expectedSignature = ComputeSignature(base64Payload);
        if (signature != expectedSignature)
            return false;

        // Decode payload using standard Base64
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

        var timestampToken = timestamp + expiresIn;

        var timestampNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timestampNextMinute = DateTimeOffset.UtcNow.AddMinutes(limitMinutes).ToUnixTimeMilliseconds();

        return timestampToken >= timestampNow && timestampToken < timestampNextMinute;
    }

    private string ComputeSignature(string data)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(data + _secretKey));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
