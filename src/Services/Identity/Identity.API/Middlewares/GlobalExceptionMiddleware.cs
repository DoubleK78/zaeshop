using System.Net;
using Common;
using Common.Enums;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Logs;
using Identity.Domain.Models.ErrorResponses;

namespace Identity.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IHostEnvironment env;

        public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            this.next = next;
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context, IServiceLogPublisher serviceLogPublisher)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 1. Attach header
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                //2. Create a description according to env
                object response = env.IsDevelopment() ?
                    new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace) :
                    new ApiResponse((int)HttpStatusCode.InternalServerError, ex.Message);

                // 3. Convert Json to cammel case
                var json = JsonSerializationHelper.Serialize(response);

                // 4. Log exption exact bug.
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                await serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Error,
                    EventName = ex.Message,
                    StackTrace = ex.StackTrace,
                    ServiceName = "Identity",
                    Environment = env.EnvironmentName,
                    Description = $"[Global Exception]: {ex.Message}",
                    IpAddress = context.Connection?.RemoteIpAddress?.ToString(),
                    StatusCode = HttpStatusCode.InternalServerError.ToString(),
                    Url = context.Request.Scheme,
                    Request = requestBody,
                    Response = json
                });

                await context.Response.WriteAsync(json);
            }
        }
    }
}
