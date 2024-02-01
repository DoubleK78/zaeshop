using Common;
using Common.Enums;
using Grpc.Core;

namespace Identity.API.Extensions
{
    public static class ServerCallContextExtensions
    {
        public static bool IsAllowedHost(this ServerCallContext context)
        {
            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            return (!isDeployed && (context.Host.Contains("localhost") || context.Host.Contains("127.0.0.1")))
                || context.Host != CommonHelper.GetServiceUrl(EServiceHost.Identity);
        }
    }
}
