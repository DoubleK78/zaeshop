using Common.Shared.Models.Users;

namespace Identity.Domain.Interfaces.Messaging
{
    public interface ISyncRolesPortalPublisher
    {
        Task SyncRolesPortalAsync(SyncRolesPortalMessage message);
    }
}
