using Common.Shared.Models.Users;
using MassTransit;
using Portal.Domain.Interfaces.Messaging;

namespace HangFireServer.Messaging.Publishers
{
    public class SyncResetExpiredRolePublisher : ISyncResetExpiredRolePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public SyncResetExpiredRolePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task SendAsync(SyncResetExpiredRoleMessage message)
        {
            await _publishEndpoint.Publish(message);
        }
    }
}
