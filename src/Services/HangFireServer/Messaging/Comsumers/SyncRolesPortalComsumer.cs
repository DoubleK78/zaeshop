using System.Text;
using Common;
using Common.Enums;
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
        private readonly IGenericRepository<UserActivityLog> _userActivityLogRepository;

        public SyncRolesPortalComsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
            _userActivityLogRepository = unitOfWork.Repository<UserActivityLog>();
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

            #region Update Subscription from new roles
            if (syncRolesMessage.IsUpdateSubscription)
            {
                // Log
                var descriptionBuilder = new StringBuilder();
                if (syncRolesMessage.OldRoleType != syncRolesMessage.NewRoleType)
                {
                    descriptionBuilder.AppendFormat("Upgrade Subscription: {0} -> {1}\n", CommonHelper.GetSubscriptionByRoleType(syncRolesMessage.OldRoleType, syncRolesMessage.Days), CommonHelper.GetSubscriptionByRoleType(syncRolesMessage.NewRoleType, syncRolesMessage.Days));
                    descriptionBuilder.AppendFormat("Role: {0} -> {1}\n", CommonHelper.GetDescription(syncRolesMessage.OldRoleType), CommonHelper.GetDescription(syncRolesMessage.NewRoleType));
                }
                else
                {
                    descriptionBuilder.AppendFormat("Extend Subscription: {0}\n", CommonHelper.GetSubscriptionByRoleType(syncRolesMessage.OldRoleType, syncRolesMessage.Days));
                }

                if (user.ExpriedRoleDate != syncRolesMessage.ExpriedRoleDate)
                {
                    descriptionBuilder.AppendFormat("ExpriedRoleDate: {0} -> {1}\n", user.ExpriedRoleDate, syncRolesMessage.ExpriedRoleDate);
                    user.ExpriedRoleDate = syncRolesMessage.ExpriedRoleDate;
                }

                var userActivityLog = new UserActivityLog
                {
                    UserId = user.Id,
                    ActivityType = EActivityType.Subscription,
                    Description = descriptionBuilder.ToString()
                };
                _userActivityLogRepository.Add(userActivityLog);
            }
            #endregion

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
