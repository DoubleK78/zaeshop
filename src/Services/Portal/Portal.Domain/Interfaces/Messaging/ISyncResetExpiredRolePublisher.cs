using Common.Shared.Models.Users;

namespace Portal.Domain.Interfaces.Messaging
{
    public interface ISyncResetExpiredRolePublisher
    {
        public Task SendAsync(SyncResetExpiredRoleMessage message);
    }
}
