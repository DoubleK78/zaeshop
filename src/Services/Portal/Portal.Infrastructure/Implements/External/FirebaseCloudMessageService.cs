using Common;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Portal.Domain.Interfaces.External;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.External
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
        private readonly FirebaseSettings _firebaseSettings;

        public FirebaseCloudMessageService(IOptions<FirebaseSettings> firebaseSettings)
        {
            _firebaseSettings = firebaseSettings.Value;

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(JsonSerializationHelper.Serialize(_firebaseSettings))
                });
            }
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

            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
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

            var response = await FirebaseMessaging.DefaultInstance.SendAllAsync(messages);
            return response.SuccessCount == messages.Count;
        }
    }
}
