using Common.Interfaces;

namespace Common.Implements;

public class RedisBackgroundBlankService : IRedisBackgroundService
{
    public RedisBackgroundBlankService()
    {
    }

    public Task<T?> GetAsync<T>(string key)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync<T>(string key, T value, int expirationMinutes)
    {
        throw new NotImplementedException();
    }
}
