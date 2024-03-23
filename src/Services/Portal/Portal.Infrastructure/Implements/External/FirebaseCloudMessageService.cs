using Common;
using Common.Enums;
using Common.Interfaces.Messaging;
using Common.Shared.Models.Logs;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Portal.Domain.Interfaces.External;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.External
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
        private readonly FirebaseSettings _firebaseSettings;
        private readonly IServiceLogPublisher _serviceLogPublisher;
        private readonly IHostEnvironment _hostingEnvironment;

        public FirebaseCloudMessageService(
            IOptions<FirebaseSettings> firebaseSettings,
            IServiceLogPublisher serviceLogPublisher,
            IHostEnvironment hostingEnvironment)
        {
            _firebaseSettings = firebaseSettings.Value;

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(JsonSerializationHelper.Serialize(_firebaseSettings))
                });
            }
            _serviceLogPublisher = serviceLogPublisher;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> SendAsync(string registrationToken, string title, string description, string? clickAction = null)
        {
            var message = new Message
            {
                Token = registrationToken,
                Data = new Dictionary<string, string?>
                    {
                        { "title", title },
                        { "body", description},
                        { "click_action", clickAction }
                    }
            };

            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;
            try
            {
                return await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (Exception ex)
            {
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Error,
                    EventName = ex.Message,
                    StackTrace = ex.StackTrace,
                    ServiceName = "Hangfire",
                    Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                    Description = $"[Exception]: SendAsync - {ex.Message}",
                    IpAddress = null,
                    StatusCode = "Internal Server Error",
                    Request = JsonSerializationHelper.Serialize(message)
                });

                return string.Empty;
            }
        }

        public async Task<bool> SendAllAsync(List<string> registrationTokens, string title, string description, string? clickAction = null)
        {
            var messages = new List<Message>();
            foreach (var registrationToken in registrationTokens)
            {
                var message = new Message
                {
                    Token = registrationToken,
                    Data = new Dictionary<string, string?>
                    {
                        { "title", title },
                        { "body", description},
                        { "click_action", clickAction }
                    }
                };

                messages.Add(message);
            }

            bool isDeployed = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT_DEPLOYED") ?? "false");
            var prefixEnvironment = isDeployed ? "[Docker] " : string.Empty;
            try
            {
                const int batchSize = 99;
                int totalBatches = (messages.Count + batchSize - 1) / batchSize;

                for (int i = 0; i < totalBatches; i++)
                {
                    int startIndex = i * batchSize;
                    int endIndex = (i + 1) * batchSize;
                    endIndex = endIndex > messages.Count ? messages.Count : endIndex;

                    var batch = messages.GetRange(startIndex, endIndex - startIndex);
                    var response = await FirebaseMessaging.DefaultInstance.SendAllAsync(batch);

                    if (response.SuccessCount != messages.Count)
                    {
                        await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                        {
                            LogLevel = ELogLevel.Error,
                            EventName = "Failed To Send All FCM Notifications",
                            ServiceName = "Hangfire",
                            Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                            Description = $"[Error]: SendAllAsync",
                            IpAddress = null,
                            StatusCode = "Internal Server Error",
                            Request = JsonSerializationHelper.Serialize(new
                            {
                                Messages = messages,
                                Responses = response.Responses.Where(o => !o.IsSuccess).Select(x => x.Exception?.Message).ToList()
                            })
                        });
                    };

                    // Delay to avoid throttling, Delay for 1 second
                    await Task.Delay(1000);
                }

                return true;
            }
            catch (Exception ex)
            {
                await _serviceLogPublisher.WriteLogAsync(new ServiceLogMessage
                {
                    LogLevel = ELogLevel.Error,
                    EventName = ex.Message,
                    StackTrace = ex.StackTrace,
                    ServiceName = "Hangfire",
                    Environment = prefixEnvironment + _hostingEnvironment.EnvironmentName,
                    Description = $"[Exception]: SendAllAsync - {ex.Message}",
                    IpAddress = null,
                    StatusCode = "Internal Server Error",
                    Request = JsonSerializationHelper.Serialize(messages)
                });

                return false;
            }
        }
    }
}
