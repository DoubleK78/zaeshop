using Common.Shared.Models.Users;
using MassTransit;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Portal.Domain.Enums;

namespace HangFireServer.Messaging.Comsumers
{
    public class SyncUserPortalComsumer : IConsumer<SyncUserPortalMessage>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<User> _userRepository;

        public SyncUserPortalComsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User>();
        }

        public async Task Consume(ConsumeContext<SyncUserPortalMessage> context)
        {
            var syncUserMessage = context.Message;

            var user = await _userRepository.GetQueryable().FirstOrDefaultAsync(o => o.IdentityUserId == syncUserMessage.IdentityUserId);
            if (user != null)
            {
                user.FullName = syncUserMessage.FullName;
                user.Avatar = syncUserMessage.IsUpdateAvatar ? syncUserMessage.Avatar : user.Avatar;
                user.Region = syncUserMessage.Region?.ToLower() == "vi" ? ERegion.vi : ERegion.en;

                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
