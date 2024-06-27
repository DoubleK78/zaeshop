using Hangfire;

namespace Portal.Infrastructure.Helpers;

public static class BackgroundJobClientExtensions
{
    public static Task EnqueueWithCircuitBreakerAsync(this IBackgroundJobClient client, Expression<Func<Task>> methodCall)
    {
        return HangfireCircuitBreaker.EnqueueAsync(() => client.Enqueue(methodCall));
    }

    public static Task EnqueueWithCircuitBreakerAsync<T>(this IBackgroundJobClient client, Expression<Func<T, Task>> methodCall)
    {
        return HangfireCircuitBreaker.EnqueueAsync(() => client.Enqueue(methodCall));
    }
}
