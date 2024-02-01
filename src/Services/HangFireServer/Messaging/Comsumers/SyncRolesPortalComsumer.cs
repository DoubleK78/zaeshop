using Common;
using Common.Shared.Models.Users;
using MassTransit;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;

namespace HangFireServer.Messaging.Comsumers
{
    public class SyncRolesPortalComsumer : IConsumer<SyncRolesPortalMessage>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<User> _userRepository;

        public SyncRolesPortalComsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
        }

        public async Task Consume(ConsumeContext<SyncRolesPortalMessage> context)
        {
            var syncRolesMessage = context.Message;

            var user = await _userRepository.GetByIdentityUserIdAsync(syncRolesMessage.IdentityUserId);
            if (user == null) return;

            if (syncRolesMessage.Roles?.Count > 0)
            {
                if (syncRolesMessage.Roles.Exists(r => r == CommonHelper.GetDescription(ERoleType.Administrator)))
                {
                    user.RoleType = ERoleType.Administrator;
                }
                else if (syncRolesMessage.Roles.Exists(r => r == CommonHelper.GetDescription(ERoleType.Partner)))
                {
                    user.RoleType = ERoleType.Partner;
                }
                else if (syncRolesMessage.Roles.Exists(r => r == CommonHelper.GetDescription(ERoleType.UserSuperPremium)))
                {
                    user.RoleType = ERoleType.UserSuperPremium;
                }
                else if (syncRolesMessage.Roles.Exists(r => r == CommonHelper.GetDescription(ERoleType.UserPremium)))
                {
                    user.RoleType = ERoleType.UserPremium;
                }
                else if (syncRolesMessage.Roles.Exists(r => r == CommonHelper.GetDescription(ERoleType.User)))
                {
                    user.RoleType = ERoleType.User;
                }
            }
            else
            {
                user.RoleType = ERoleType.User;
            }

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
