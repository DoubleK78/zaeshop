using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HangFireServer.HealthCheck;

public class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
    }
}
