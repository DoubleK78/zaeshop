using Common.Shared.Models.Users;
using Identity.Domain.Interfaces.Messaging;
using MassTransit;

namespace Identity.Infrastructure.Implements.Messaging
{
    public class SyncRolesPortalPublisher : ISyncRolesPortalPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public SyncRolesPortalPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task SyncRolesPortalAsync(SyncRolesPortalMessage message)
        {
            await _publishEndpoint.Publish(message);
        }
    }
}
