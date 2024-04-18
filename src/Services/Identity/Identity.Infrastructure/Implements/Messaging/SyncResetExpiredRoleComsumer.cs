using Common.Shared.Models.Users;
using Identity.Domain.AggregatesModel.UserAggregate;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Implements.Messaging
{
    public class SyncResetExpiredRoleComsumer : IConsumer<SyncResetExpiredRoleMessage>
    {
        private readonly AppIdentityDbContext _context;
        private readonly UserManager<User> _userManager;
        public SyncResetExpiredRoleComsumer(AppIdentityDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task Consume(ConsumeContext<SyncResetExpiredRoleMessage> context)
        {
            var syncResetExpiredRoleMessage = context.Message;
            var users = await _context.Users.Where(o => syncResetExpiredRoleMessage.UserIds.Contains(o.Id)).ToListAsync();
            foreach (var user in users)
            {
                if (user.ExpriedRoleDate <= DateTime.UtcNow)
                {
                    user.ExpriedRoleDate = null;

                    var userRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                    await _userManager.AddToRoleAsync(user, "User");
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
